using ParkingSystem.AppCore.Entities;

namespace ParkingSystem.AppCore.Dtos;

public record CreateTariffRequest(
    string Name, 
    TariffType Type, 
    int FreeMinutes, 
    decimal HourlyRate, 
    decimal MaxDailyRate);

public record UpdateTariffRequest(
    string Name, 
    int FreeMinutes, 
    decimal HourlyRate, 
    decimal MaxDailyRate);