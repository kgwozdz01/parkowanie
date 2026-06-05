using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ParkingSystem.Infrastructure.Security;

public class JwtSettings
{
    private string? _secret;
    private string? _issuer;
    private string? _audience;

    public string Secret
    {
        get => _secret ?? "SuperTajnyIBezpiecznyKluczDoGenerowaniaTokenowJWT2026!";
        set => _secret = value;
    }

    public string Issuer
    {
        get => _issuer ?? "ParkingSystemAPI";
        set => _issuer = value;
    }

    public string Audience
    {
        get => _audience ?? "ParkingSystemSPA";
        set => _audience = value;
    }

    public int ExpiryInMinutes { get; set; } = 60;
    
    // Brakująca właściwość dla AuthService
    public int RefreshTokenDays { get; set; } = 7;

    // Brakująca metoda pomocnicza, której szuka AuthService
    public SymmetricSecurityKey GetSymmetricKey()
    {
        var keyBytes = Encoding.UTF8.GetBytes(Secret);
        return new SymmetricSecurityKey(keyBytes);
    }
}