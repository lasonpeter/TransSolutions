using TransSolutions.Shared.Enums.Auth;

namespace TransSolutions.Shared.Contracts.Auth;

public record RegisterRequest(string Email, string Password, string Name, string Surname, UserRole Role);

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken, string AccessToken);
public record GetUsersRequest(string? FullName = null, int PageNumber = 1, int PageSize = 10);
public record UpdateUserRequest(string? Name, string? Surname, string? Password);

