using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.Infrastructure.Persistence;
using ParkingSystem.Infrastructure.Security;

namespace ParkingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Konfiguracja Bazy Danych (EF Core + SQLite)
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=../ParkingSystem.Infrastructure/parking.db";
            
        services.AddDbContext<ParkingSystemDbContext>(options =>
            options.UseSqlite(connectionString));

        // 2. Konfiguracja Identity (Użytkownicy i Role)
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ParkingSystemDbContext>()
        .AddDefaultTokenProviders();

        // 3. Konfiguracja JWT Settings (Bezpieczne bindowanie z wartościami awaryjnymi)
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        
        // REJESTRACJA DLA REPOZYTORIÓW / SERWISÓW (Wstrzykiwanie czystej klasy JwtSettings)
        services.AddSingleton(jwtSettings);

        // Rejestrujemy jako IOptions dla reszty aplikacji (np. dla AddJwtBearer)
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // 4. Konfiguracja Uwierzytelniania JWT Bearer
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

        // 5. Autoryzacja i Polityki (Jeśli masz przypisane role w kontrolerach)
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireOperatorRole", policy => policy.RequireRole("Operator"));
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
        });

        // 6. Rejestracja serwisów aplikacyjnych
        services.AddScoped<ParkingSystem.AppCore.Services.IAuthService, ParkingSystem.Infrastructure.Security.AuthService>();

        return services;
        
    }
}