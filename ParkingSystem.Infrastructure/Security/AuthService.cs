using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ParkingSystem.AppCore.Constants;
using ParkingSystem.AppCore.DTOs;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.AppCore.Services;
using ParkingSystem.Infrastructure.Persistence;

namespace ParkingSystem.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ParkingSystemDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ParkingSystemDbContext context,
        JwtSettings jwtSettings)
    {
        _userManager = userManager;
        _context = context;
        _jwtSettings = jwtSettings;
    }

    // ── Logowanie ─────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Nieprawidłowy email lub hasło.");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedAccessException("Nieprawidłowy email lub hasło.");
        }

        if (user.Status != UserStatuses.Active)
            throw new InvalidOperationException("Konto operatora/administratora jest nieaktywne.");

        if (await _userManager.IsLockedOutAsync(user))
            throw new InvalidOperationException("Konto zostało zablokowane z powodu zbyt wielu nieudanych prób.");

        await _userManager.ResetAccessFailedCountAsync(user);

        return await GenerateAuthResponseAsync(user);
    }

    // ── Odświeżenie tokenu ────────────────────────────────
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        
        var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new SecurityTokenException("Nieprawidłowy token dostępowy.");

        if (!Guid.TryParse(userIdString, out var userId))
            throw new SecurityTokenException("Nieprawidłowy identyfikator użytkownika.");

        var user = await _userManager.FindByIdAsync(userIdString)
            ?? throw new KeyNotFoundException("Użytkownik nie istnieje w systemie.");

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && t.ApplicationUserId == userId)
            ?? throw new SecurityTokenException("Nieprawidłowy refresh token.");

        if (!refreshToken.IsActive)
            throw new SecurityTokenException("Refresh token wygasł lub został już wykorzystany.");

        // Oznaczamy stary token jako zużyty
        refreshToken.IsUsed = true;
        refreshToken.IsRevoked = true;

        var newResponse = await GenerateAuthResponseAsync(user);
        await _context.SaveChangesAsync();

        return newResponse;
    }

    // ── Wylogowanie / Odwołanie sesji ─────────────────────
    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken)
            ?? throw new KeyNotFoundException("Podany token nie istnieje.");

        if (!token.IsActive)
            throw new InvalidOperationException("Token jest już nieaktywny.");

        token.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    // ── Metody pomocnicze (Generowanie Tokenów) ───────────
    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        var userDto = new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Status,
            roles
        );

        return new AuthResponseDto(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            userDto
        );
    }

    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("status", user.Status),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var handler = new JsonWebTokenHandler();
        var credentials = new SigningCredentials(_jwtSettings.GetSymmetricKey(), SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            SigningCredentials = credentials
        };

        return handler.CreateToken(tokenDescriptor);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        // Opcjonalnie: Unieważniamy poprzednie aktywne tokeny użytkownika (czyszczenie sesji)
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.ApplicationUserId == userId && !t.IsRevoked && !t.IsUsed)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        var newRefreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            ApplicationUserId = userId
        };

        await _context.RefreshTokens.AddAsync(newRefreshToken);
        await _context.SaveChangesAsync();

        return newRefreshToken;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Ignorujemy wygasanie – chcemy odczytać dane z wygasłego tokenu
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = _jwtSettings.GetSymmetricKey()
        };

        var handler = new JsonWebTokenHandler();
        var result = handler.ValidateToken(accessToken, validationParameters);

        if (!result.IsValid)
            throw new SecurityTokenException("Nieprawidłowy lub zmanipulowany token dostępowy.");

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}