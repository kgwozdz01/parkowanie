using ParkingSystem.AppCore.Common;

namespace ParkingSystem.AppCore.Entities;

public class RefreshToken : EntityBase
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    
    // Klucz obcy do użytkownika
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    // Właściwość pomocnicza sprawdzająca, czy token stracił ważność
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    // Token jest aktywny, jeśli nie wygasł, nie został użyty i nie został unieważniony
    public bool IsActive => !IsExpired && !IsUsed && !IsRevoked;
}