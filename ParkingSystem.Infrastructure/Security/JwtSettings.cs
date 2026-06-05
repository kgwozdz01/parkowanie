using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ParkingSystem.Infrastructure.Security;

public class JwtSettings(IConfiguration configuration)
{
    private static readonly string Section = "Jwt"; 

    public string Issuer => configuration.GetSection(Section).GetSection("Issuer").Value 
                            ?? throw new InvalidOperationException("JWT Issuer is not set in configuration.");
    
    public string Audience => configuration.GetSection(Section).GetSection("Audience").Value 
                              ?? throw new InvalidOperationException("JWT Audience is not set in configuration.");
    
    public string SecretKey => configuration.GetSection(Section).GetSection("SecretKey").Value 
                               ?? throw new InvalidOperationException("JWT SecretKey is not set in configuration.");
    
    public int ExpiryInMinutes => configuration.GetSection(Section).GetSection("ExpiryInMinutes").Get<int>();
    public int RefreshTokenDays => configuration.GetSection(Section).GetSection("RefreshTokenDays").Get<int>();

    public SymmetricSecurityKey GetSymmetricKey() =>
        new(Encoding.UTF8.GetBytes(SecretKey));
}