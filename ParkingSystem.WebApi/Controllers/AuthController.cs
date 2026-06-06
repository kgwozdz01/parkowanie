using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.AppCore.DTOs;
using ParkingSystem.AppCore.Services;

namespace ParkingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Logowanie do systemu – zwraca Access Token oraz Refresh Token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous] // Dowolny użytkownik musi mieć dostęp do tego punktu bez tokenu
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Odświeżenie wygasłego Access Tokenu na podstawie ważnego Refresh Tokenu.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(dto);
            return Ok(result);
        }
        catch (Exception)
        {
            return Unauthorized(new { message = "Nieprawidłowy lub wygasły token odświeżania." });
        }
    }

    /// <summary>
    /// Wylogowanie z systemu – unieważnia podany Refresh Token.
    /// </summary>
    [HttpPost("revoke")]
    [Authorize] // Tylko zalogowany użytkownik może ubić swoją sesję
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        try
        {
            await _authService.RevokeTokenAsync(refreshToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Zwraca profil i role aktualnie zalogowanego użytkownika na podstawie przesłanego tokenu.
    /// </summary>
    [HttpGet("me")]
    [Authorize] // Wymaga ważnego tokenu dostępowego
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        // Wyciągamy dane użytkownika bezpośrednio z Claims zaszytych w tokenie JWT
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        var lastName = User.FindFirstValue(ClaimTypes.Surname);
        var status = User.FindFirstValue("status");
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        if (userId == null || email == null)
        {
            return BadRequest(new { message = "Token nie zawiera wymaganych informacji profilowych." });
        }

        var userProfile = new UserDto(
            Guid.Parse(userId),
            email,
            firstName ?? string.Empty,
            lastName ?? string.Empty,
            status ?? "Unknown",
            roles
        );

        return Ok(userProfile);
    }
}