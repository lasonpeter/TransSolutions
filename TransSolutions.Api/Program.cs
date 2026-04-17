
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Infrastructure.DbContext;
using TransSolutions.Infrastructure.Repositories;
using TransSolutions.Infrastructure.Services; // Ensure this is present
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IDriverService,DriverService>();
builder.Services.AddScoped<IVehicleService,VehicleService>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRoadTripRepository, RoadTripRepository>();
builder.Services.AddScoped<IRoadTripService, RoadTripService>();



JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthenticationJwtBearer(
    _ => { }, 
    bearerOptions =>
    {
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints().SwaggerDocument().AddResponseCaching();

var app = builder.Build();

// non production code just for simpler testing for you guys
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();                                                                
app.UseFastEndpoints();                                                                
app.UseDefaultExceptionHandler();

app.MapFallback(async (HttpContext ctx) =>
{
    // Custom logic here if needed
});
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.Run();