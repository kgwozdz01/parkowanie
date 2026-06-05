using ParkingSystem.AppCore.DTOs;

namespace ParkingSystem.AppCore.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task RevokeTokenAsync(string refreshToken);
}