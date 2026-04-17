using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Domain.Models.BusinessLogic;

namespace TransSolutions.Infrastructure.DbContext.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    private const string AdminId = "00000000-0000-0000-0000-000000000001";

    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(e => e.FullNameComputed)
            .HasComputedColumnSql("\"Name\" || ' ' || \"Surname\"", stored: true)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(x => x.Driver)
            .WithOne(x => x.User)
            .HasForeignKey<Driver>(x => x.AppUserId)
            .OnDelete(DeleteBehavior.NoAction);
        var admin = new AppUser
        {
            Id = AdminId,
            UserName = "admin@transsolutions.com",
            NormalizedUserName = "ADMIN@TRANSSOLUTIONS.COM",
            Email = "admin@transsolutions.com",
            NormalizedEmail = "ADMIN@TRANSSOLUTIONS.COM",
            EmailConfirmed = true,
            Name = "System",
            Surname = "Administrator",
            SecurityStamp = "550e8400-e29b-41d4-a716-446655440000"
        };

        var passwordHasher = new PasswordHasher<AppUser>();
        admin.PasswordHash = passwordHasher.HashPassword(admin, "8gBd32Za-if75hg");

        builder.HasData(admin);
    }
}