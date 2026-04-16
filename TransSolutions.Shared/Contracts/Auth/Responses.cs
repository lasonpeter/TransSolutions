namespace TransSolutions.Shared.Contracts.Auth;

public record RegisterResponse(string Token);
public record LoginResponse(string Token, string RefreshToken, long ExpiresIn);
public record RefreshResponse(string Token, string RefreshToken, long ExpiresIn);
