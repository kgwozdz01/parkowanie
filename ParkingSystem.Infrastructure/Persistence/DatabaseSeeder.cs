using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ParkingSystem.AppCore.Entities;

namespace ParkingSystem.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ParkingSystemDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        // 1. Zapewnienie istnienia roli Admin
        const string adminRoleName = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRoleName));
        }

        // 2. Zapewnienie istnienia konta Administratora
        const string adminEmail = "admin@parkowanko.pl";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                // Wymagane właściwości dla ApplicationUser w Twoim projekcie:
                FirstName = "System",
                LastName = "Administrator",
                Status = "Active" // Jeśli Status to enum, np. UserStatus.Active, zamień na odpowiedni typ
            };

            var createAdminResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }

        // 3. Dodanie domyślnej taryfy startowej (jeśli tabela jest pusta)
        if (!context.Tariffs.Any())
        {
            var defaultTariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Standardowa Taryfa Miejska",
                Type = TariffType.Standard, // Użycie typu enum zamiast stringa
                FreeMinutes = 15,
                HourlyRate = 4.50m,
                MaxDailyRate = 50.00m,
                IsActive = true,
                CreatedBy = adminUser?.Id ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            context.Tariffs.Add(defaultTariff);
        }

        // 4. Dodanie domyślnych bramek (jeśli tabela jest pusta)
        if (!context.Gates.Any())
        {
            context.Gates.AddRange(
                new Gate
                {
                    Id = Guid.NewGuid(),
                    Name = "Szlaban Wjazdowy Główny",
                    Type = GateType.Entry, // Użycie typu enum zamiast stringa
                    Location = "Sektor A - ul. Parkingowa",
                    IsOperational = true
                },
                new Gate
                {
                    Id = Guid.NewGuid(),
                    Name = "Szlaban Wyjazdowy Główny",
                    Type = GateType.Exit, // Użycie typu enum zamiast stringa
                    Location = "Sektor A - ul. Parkingowa",
                    IsOperational = true
                }
            );
        }

        await context.SaveChangesAsync();
    }
}