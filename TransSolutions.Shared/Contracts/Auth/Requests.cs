namespace TransSolutions.Shared.Contracts.Auth;

public record RegisterRequest(string Email, string Password, string Name, string Surname);

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken, string AccessToken);

