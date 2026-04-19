
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Exceptions;
using TransSolutions.Infrastructure.DbContext;
using TransSolutions.Infrastructure.Repositories;
using TransSolutions.Infrastructure.Services; // Ensure this is present
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

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
        bearerOptions.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                string authHeader = context.Request.Headers.Authorization;
    
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
                
                else if (context.Request.Cookies.TryGetValue("AccessToken", out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
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


//CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Your React port
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowViteApp");

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();                                                                
app.UseFastEndpoints();                                                                
app.UseCustomExceptionHandler();

app.MapFallback(async (HttpContext ctx) =>
{
    // Custom logic here if needed
});
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.Run();