using HelpDesk.Api.Models;

namespace HelpDesk.Api.Dtos;

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    UserResponse User);

public record UserResponse(
    int Id,
    string Name,
    string Email,
    UserRole Role);
