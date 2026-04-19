using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Shared.CustomClaims;

namespace TransSolutions.Infrastructure.DbContext;

public class AppDbContext : IdentityDbContext<AppUser>
{
    private const string AdminId = "00000000-0000-0000-0000-000000000001";
    public DbSet<RefreshTokens> RefreshTokens { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<RoadTrip> RoadTrips { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<RefreshTokens>(entity => 
        {
            entity.HasIndex(e => e.Token).IsUnique();

            entity.HasOne(d => d.User)
                .WithMany() 
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasMany(x => x.RoadTrips)
                .WithOne(x => x.Driver)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasMany(x => x.RoadTrips)
                .WithOne(x => x.Vehicle)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<IdentityUserClaim<string>>().HasData(new IdentityUserClaim<string>
        {
            Id = 2,
            UserId = AdminId,
            ClaimType = CustomClaims.AdminClaim,
            ClaimValue = "true"
        });
    }
}