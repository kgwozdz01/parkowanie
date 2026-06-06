using ParkingSystem.AppCore.Entities;

namespace ParkingSystem.AppCore.Dtos;

public record RegisterGateRequest(string Name, GateType Type, string Location);