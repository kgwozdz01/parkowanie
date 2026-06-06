using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.Infrastructure.Persistence;
using ParkingSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. REJESTRACJA USŁUG W KONTENERZE IoC (Dependency Injection)
// =========================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Domyślna, minimalistyczna konfiguracja Swaggera - bezkonfliktowa z OpenAPI v2.3.0
builder.Services.AddSwaggerGen();

// Konfiguracja połączenia z bazą danych SQLite
builder.Services.AddDbContext<ParkingSystemDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Data Source=../ParkingSystem.Infrastructure/parking.db"));

// Konfiguracja ASP.NET Core Identity dla autoryzacji użytkowników
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ParkingSystemDbContext>()
    .AddDefaultTokenProviders();

// Konfiguracja uwierzytelniania tokenami JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SuperTajnyIUltraBezpiecznyKluczSzyfrujacy2026!");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "ParkingSystemServer",
            ValidAudience = jwtSettings["Audience"] ?? "ParkingSystemClients",
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Rejestracja serwisu biletowego i kalkulatora opłat
builder.Services.AddScoped<TicketService>();

// =========================================================================
// 2. POTOK PRZETWARZANIA ŻĄDAŃ (Middleware Pipeline)
// =========================================================================

var app = builder.Build();

// Precyzyjna konfiguracja Swaggera dla środowiska deweloperskiego
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkingSystem API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =========================================================================
// 3. AUTOMATYCZNE URUCHOMIENIE MIGRACJI I SEEDERA DANYCH
// =========================================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ParkingSystemDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Wystąpił krytyczny błąd podczas migracji lub seedowania bazy danych.");
    }
}

app.Run();