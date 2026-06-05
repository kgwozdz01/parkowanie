using ParkingSystem.AppCore.Common;

namespace ParkingSystem.AppCore.Entities;

public class ParkingGate : EntityBase
{
    public required string Name { get; set; }
    public required string Location { get; set; }
    public bool IsOpen { get; set; }
    public bool IsActive { get; set; }

    // Relacja: Jedna bramka może mieć wiele zapisanych zdjęć z kamer
    public ICollection<GateCameraImage> CameraImages { get; set; } = new List<GateCameraImage>();
}