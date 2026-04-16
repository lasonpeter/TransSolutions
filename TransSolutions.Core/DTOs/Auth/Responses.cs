namespace TransSolutions.Domain.DTOs.Auth;

public record RefreshResponseDto(string Token, string RefreshToken, long ExpiresIn, string Jti);
