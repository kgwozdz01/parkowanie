namespace ParkingSystem.AppCore.DTOs;

public record LoginDto(
    string Email, 
    string Password
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    IEnumerable<string> Roles
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenDto(
    string AccessToken,
    string RefreshToken
);