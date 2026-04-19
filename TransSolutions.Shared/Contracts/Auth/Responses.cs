namespace TransSolutions.Shared.Contracts.Auth;

public record RegisterResponse(string Token);
public record LoginResponse(string Token, string RefreshToken, long ExpiresIn);
public record RefreshResponse(string Token, string RefreshToken, long ExpiresIn);

public record GetUserResponse(string Id, string Email, string Name, string Surname);
public record GetUsersResponse(IEnumerable<GetUserResponse> Users, int TotalCount);
