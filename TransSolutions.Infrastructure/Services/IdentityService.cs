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
            JwtId = authData.Jti, 
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

    public async Task<GetUsersResponse> GetUsers(GetUsersRequest request, CancellationToken ct)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            query = query.Where(x => EF.Functions.ILike(x.FullNameComputed, $"%{request.FullName}%"));
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderBy(x => x.Surname)
            .Skip(request.PageSize * (request.PageNumber - 1))
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new GetUsersResponse(
            Users: users.Select(u => new GetUserResponse(u.Id, u.Email ?? string.Empty, u.Name, u.Surname)),
            TotalCount: totalCount
        );
    }

    public async Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserRequest request, ClaimsPrincipal currentUser)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        if (!string.IsNullOrWhiteSpace(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Surname))
            user.Surname = request.Surname;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return result;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // Managers cannot change Admin's password
            if (currentUser.HasClaim(c => c.Type == CustomClaims.ManagerClaim) && 
                !currentUser.HasClaim(c => c.Type == CustomClaims.AdminClaim))
            {
                var targetClaims = await _userManager.GetClaimsAsync(user);
                if (targetClaims.Any(c => c.Type == CustomClaims.AdminClaim))
                {
                    return IdentityResult.Failed(new IdentityError 
                    { 
                        Code = "Forbidden", 
                        Description = "Managers are not allowed to change Admin passwords." 
                    });
                }
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, token, request.Password);
        }

        return result;
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
        var handler = new JsonWebTokenHandler(); 
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Backup check for Driver if not in claims
        if (claims.All(c => c.Type != CustomClaims.DriverClaim))
        {
            var isDriver = await _context.Users.Where(x => x.Id == user.Id).AsNoTracking().Select(x => x.Driver != null)
                .FirstOrDefaultAsync();
            if (isDriver)
                claims.Add(new Claim(CustomClaims.DriverClaim, "true"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
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