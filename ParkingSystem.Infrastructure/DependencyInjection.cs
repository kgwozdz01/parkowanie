using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ParkingSystem.AppCore.Authorization;
using ParkingSystem.AppCore.Constants;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.AppCore.Services;
using ParkingSystem.Infrastructure.Persistence;
using ParkingSystem.Infrastructure.Security;

namespace ParkingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Rejestracja bazy danych SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ParkingSystemDbContext>(options =>
            options.UseSqlite(connectionString));

        // 2. Rejestracja konfiguracji i serwisu JWT
        services.AddSingleton<JwtSettings>();
        services.AddScoped<IAuthService, AuthService>();

        // 3. Konfiguracja ASP.NET Core Identity dla użytkowników systemu
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Konfiguracja wymagań dotyczących haseł
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Blokada konta po nieudanych próbach logowania
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ParkingSystemDbContext>();

        // 4. Pobranie instancji ustawień JWT na potrzeby konfiguracji uwierzytelniania
        var jwtSettings = new JwtSettings(configuration);

        // 5. Rejestracja i konfiguracja silnika uwierzytelniania JWT Bearer
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
                    IssuerSigningKey = jwtSettings.GetSymmetricKey(),
                    ClockSkew = TimeSpan.Zero // Brak tolerancji czasowej – natychmiastowe wygasanie co do sekundy
                };
            });

        // 6. Definicja polityk bezpieczeństwa (Zgodnie z Lab 7, punkt 6)
        services.AddAuthorization(options =>
        {
            // Polityka bazująca na roli Administratora
            options.AddPolicy(AppPolicies.AdminOnly.Name(), policy =>
                policy.RequireRole(UserRoles.Administrator));

            // Polityka bazująca na roli Operatora Szlabanów
            options.AddPolicy(AppPolicies.OperatorOnly.Name(), policy =>
                policy.RequireRole(UserRoles.Operator));

            // Złożona polityka: Użytkownik musi być zalogowany i posiadać status "Active" w tokenie
            options.AddPolicy(AppPolicies.ActiveUser.Name(), policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("status", UserStatuses.Active));

            // Domyślna polityka – każdy punkt bez jawnego określenia polityki wymaga zalogowania
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Polityka Fallback – jeśli kontroler zapomni atrybutu [Authorize], system i tak zablokuje dostęp
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}