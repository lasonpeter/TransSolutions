using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Infrastructure.DbContext;
using TransSolutions.Infrastructure.Services;
using TransSolutions.Shared.Contracts.Auth;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Reflection;
using TransSolutions.Shared.CustomClaims;

namespace TransSolutions.Testing.Services;

public class IdentityServiceTests
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var userStoreMock = new Mock<IUserStore<AppUser>>();
        
        userStoreMock.As<IUserEmailStore<AppUser>>()
            .Setup(s => s.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string email, CancellationToken ct) => 
                _context.Users.FirstOrDefault(u => u.NormalizedEmail == email.ToUpper()));

        userStoreMock.As<IUserPasswordStore<AppUser>>()
            .Setup(s => s.GetPasswordHashAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser u, CancellationToken ct) => u.PasswordHash);

        userStoreMock.As<IUserClaimStore<AppUser>>()
            .Setup(s => s.GetClaimsAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        var userManagerMock = new Mock<UserManager<AppUser>>(
            userStoreMock.Object, null!, new PasswordHasher<AppUser>(), null!, null!, null!, null!, null!, null!);

        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _context.Users.FirstOrDefault(u => u.Id == id));

        userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<AppUser>()))
            .ReturnsAsync("reset-token");

        userManagerMock.Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => _context.Users.FirstOrDefault(u => u.NormalizedEmail == email.ToUpper()));

        userManagerMock.Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync((AppUser u, string p) => 
                new PasswordHasher<AppUser>().VerifyHashedPassword(u, u.PasswordHash!, p) == PasswordVerificationResult.Success);

        userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new List<Claim>());

        _userManager = userManagerMock.Object;

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("key-that-is-at-least-32-chars-yes");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("issuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("audience");

        _sut = new IdentityService(_userManager, _context, _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnResponse_WhenCredentialsAreValid()
    {
        var password = "password123";
        var user = new AppUser 
        { 
            Id = Guid.NewGuid().ToString(), 
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            UserName = "test@test.com",
            NormalizedUserName = "TEST@TEST.COM",
            Name = "Test",
            Surname = "User"
        };
        
        typeof(AppUser).GetProperty(nameof(AppUser.FullNameComputed))
            ?.SetValue(user, "Test User");

        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.LoginAsync(user.Email, password);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);
        
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
        Assert.NotNull(storedToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Act
        var result = await _sut.LoginAsync("nonexistent567h@test.com", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeTokens()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = new RefreshTokens
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "hashed-token",
            IsRevoked = false,
            IsUsed = false,
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.LogoutAsync(userId);

        // Assert
        Assert.True(result);
        var updatedToken = await _context.RefreshTokens.FindAsync(token.Id);
        Assert.True(updatedToken!.IsRevoked);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPaginatedList()
    {
        // Arrange
        var users = new List<AppUser>
        {
            new AppUser { Id = Guid.NewGuid().ToString(), Name = "User1", Surname = "A", Email = "u1@test.com" },
            new AppUser { Id = Guid.NewGuid().ToString(), Name = "User2", Surname = "B", Email = "u2@test.com" }
        };

        foreach (var user in users)
        {
            typeof(AppUser).GetProperty(nameof(AppUser.FullNameComputed))
                ?.SetValue(user, $"{user.Name} {user.Surname}");
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        var request = new GetUsersRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var response = await _sut.GetUsers(request, CancellationToken.None);

        // Assert
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Users.Count());
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateDetailsAndReturnSuccess()
    {
        // Arrange
        var user = new AppUser { Id = Guid.NewGuid().ToString(), Name = "Old", Surname = "Name" };
        typeof(AppUser).GetProperty(nameof(AppUser.FullNameComputed))
            ?.SetValue(user, "Old Name");
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new UpdateUserRequest("NewName", "NewSurname", "NewPassword123");
        var currentUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(CustomClaims.AdminClaim, "true") }, "TestAuth"));

        // Act
        var result = await _sut.UpdateUserAsync(user.Id, request, currentUser);

        // Assert
        Assert.True(result.Succeeded);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.Equal("NewName", updatedUser!.Name);
        Assert.Equal("NewSurname", updatedUser.Surname);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailure_WhenManagerTriesToChangeAdminPassword()
    {
        // Arrange
        var adminUser = new AppUser { Id = Guid.NewGuid().ToString(), Name = "Admin", Surname = "User" };
        typeof(AppUser).GetProperty(nameof(AppUser.FullNameComputed))
            ?.SetValue(adminUser, "Admin User");
        
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        var request = new UpdateUserRequest("Name", "Surname", "NewPassword123");
        var managerUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(CustomClaims.ManagerClaim, "true") }, "TestAuth"));

        // Mock GetClaimsAsync to return AdminClaim for the target user
        Mock.Get(_userManager).Setup(m => m.GetClaimsAsync(It.Is<AppUser>(u => u.Id == adminUser.Id)))
            .ReturnsAsync(new List<Claim> { new Claim(CustomClaims.AdminClaim, "true") });

        // Act
        var result = await _sut.UpdateUserAsync(adminUser.Id, request, managerUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "Managers are not allowed to change Admin passwords.");
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var request = new UpdateUserRequest("Name", "Surname", null);
        var currentUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(CustomClaims.AdminClaim, "true") }, "TestAuth"));

        // Act
        var result = await _sut.UpdateUserAsync(Guid.NewGuid().ToString(), request, currentUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "User not found.");
    }
}
