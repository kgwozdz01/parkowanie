using System;

namespace ParkingSystem.AppCore.Entities;

public enum TariffType
{
    Standard,
    Weekend,
    Night,
    Subscription
}

public class Tariff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TariffType Type { get; set; } = TariffType.Standard;
    
    // Konfiguracja finansowa
    public int FreeMinutes { get; set; } // np. pierwsze 15 min darmowe
    public decimal HourlyRate { get; set; }
    public decimal MaxDailyRate { get; set; }
    
    // Status operacyjny
    public bool IsActive { get; set; } = true;
    
    // Śledzenie zmian (kto i kiedy)
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}