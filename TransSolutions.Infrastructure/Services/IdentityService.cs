using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TransSolutions.Application.DTOs.Auth;
using TransSolutions.Domain.DTOs.Auth;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Infrastructure.DbContext;
using TransSolutions.Shared.Contracts.Auth;
using TransSolutions.Shared.CustomClaims;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
namespace TransSolutions.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser>  _userManager;
    private readonly AppDbContext _context; 
    private readonly IConfiguration _config;
    private readonly string _jwtKey;
    public IdentityService(UserManager<AppUser> userManager, AppDbContext context, IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
        _jwtKey = config["Jwt:Key"];
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;
        
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid) return null;
        var authData = await GenerateAuthResponse(user);
        
        var hashedRefreshToken = _userManager.PasswordHasher.HashPassword(user, authData.RefreshToken);

        var refreshTokenEntry = new RefreshTokens 
        { 
            Id = Guid.NewGuid(),
            Token = hashedRefreshToken, 
            JwtId = authData.Jti, // Now we are tracking which JWT this belongs to
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(1), 
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
            IsRevoked = false
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntry);
        await _context.SaveChangesAsync();

        return new LoginResponse(
            Token: authData.Token,
            RefreshToken: authData.RefreshToken,
            ExpiresIn: 15 * 60
        );
    }

    public async Task<RefreshResponse?> RefreshTokenAsync(string accessToken, string refreshToken)
    { 
        var principal = GetPrincipalFromExpiredToken(accessToken);
        var userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        if (string.IsNullOrEmpty(userId)) 
            throw new Exception("User not found ");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) 
            throw new Exception("User not found ");

        var storedTokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsUsed && !x.IsRevoked)
            .ToListAsync();

        var validStoredToken = storedTokens.FirstOrDefault(t => 
            _userManager.PasswordHasher.VerifyHashedPassword(user, t.Token, refreshToken) 
            == PasswordVerificationResult.Success);

        if (validStoredToken == null || DateTime.UtcNow > validStoredToken.ExpiryDate)
            throw new Exception("Invalid refresh token");

        validStoredToken.IsUsed = true;
        _context.RefreshTokens.Update(validStoredToken);

        var newResponse = await GenerateAuthResponse(user);
        
        var newHashedToken = _userManager.PasswordHasher.HashPassword(user, newResponse.RefreshToken);
        await _context.RefreshTokens.AddAsync(new RefreshTokens 
        { 
            Token = newHashedToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        });

        await _context.SaveChangesAsync();
        var response =new RefreshResponse(newResponse.Token, newResponse.RefreshToken, newResponse.ExpiresIn);
        return response;    
    }
    
    
    public async Task<bool> LogoutAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked && !x.IsUsed)
            .ToListAsync();

        if(!tokens.Any())
            throw new Exception("No refresh tokens found");
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        _context.RefreshTokens.UpdateRange(tokens);
        return await _context.SaveChangesAsync() > 0;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false 
        }, out _);
        return principal;
    }

    private async Task<RefreshResponseDto> GenerateAuthResponse(AppUser user)
    {
        string jwtId = Guid.NewGuid().ToString();
        var refreshToken = GenerateRefreshToken();
        var token = await GenerateAccessToken(user);
        var response = new RefreshResponseDto(Token:token, RefreshToken:refreshToken, ExpiresIn: 15*60, Jti:jwtId);
        return response;
    }
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    private async Task<string> GenerateAccessToken(AppUser user)
    {
        var isDriver = _context.Users.Where(x => x.Id == user.Id).AsNoTracking().Select(x => x.Driver != null)
            .FirstOrDefaultAsync();
        var handler = new JsonWebTokenHandler(); 
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var claims = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

        });
        if (await isDriver)
            claims.AddClaim(new Claim(CustomClaims.DriverClaim, "true"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature) 
        };

        return handler.CreateToken(tokenDescriptor);
    }
}