using System;

namespace ParkingSystem.AppCore.Entities;

public enum GateType
{
    Entry, // Bramka wjazdowa (szlaban wjazdowy)
    Exit   // Bramka wyjazdowa (szlaban wyjazdowy)
}

public class Gate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // np. "Bramka Północna A"
    public GateType Type { get; set; }
    public string Location { get; set; } = string.Empty; // np. "Poziom 0, Al. Pokoju"
    
    // Status fizycznego szlabanu / urządzenia
    public bool IsOperational { get; set; } = true; // true = włączona, false = wyłączona/awaria
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}