using ParkingSystem.AppCore.Common;

namespace ParkingSystem.AppCore.Entities;

public class GateCameraImage : EntityBase
{
    public required string ImagePath { get; set; }
    public required string RecognizedLicensePlate { get; set; }
    public double Confidence { get; set; } // Pewność rozpoznania tablicy (np. 0.95 dla 95%)

    // Klucz obcy i właściwość nawigacyjna do bramki
    public Guid ParkingGateId { get; set; }
    public ParkingGate? ParkingGate { get; set; }
}