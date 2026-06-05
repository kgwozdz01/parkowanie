using Microsoft.AspNetCore.Identity;

namespace ParkingSystem.AppCore.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Status { get; set; } = Constants.UserStatuses.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relacja: Użytkownik może mieć wiele wygenerowanych tokenów odświeżania
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}