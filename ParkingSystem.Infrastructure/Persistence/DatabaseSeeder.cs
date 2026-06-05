using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.AppCore.Constants;
using ParkingSystem.AppCore.Entities;

namespace ParkingSystem.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ParkingSystemDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        // 1. Automatyczne wykonanie migracji i utworzenie bazy danych na dysku, jeśli nie istnieje
        await context.Database.MigrateAsync();

        // 2. Tworzenie ról w systemie (jeśli jeszcze nie istnieją)
        string[] roles = [UserRoles.Administrator, UserRoles.Operator];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }
        }

        // 3. Tworzenie domyślnego konta Administratora
        var adminEmail = "admin@parkingsystem.pl";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Jan",
                LastName = "Kowalski",
                Status = UserStatuses.Active,
                EmailConfirmed = true
            };

            // Hasło spełnia wymagania z naszej konfiguracji DI (min. 8 znaków, duża/mała litera, cyfra, znak specjalny)
            var result = await userManager.CreateAsync(adminUser, "SecureAdmin2026!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Administrator);
            }
        }

        // 4. Tworzenie domyślnego konta Operatora bramek
        var operatorEmail = "operator@parkingsystem.pl";
        if (await userManager.FindByEmailAsync(operatorEmail) == null)
        {
            var operatorUser = new ApplicationUser
            {
                UserName = operatorEmail,
                Email = operatorEmail,
                FirstName = "Piotr",
                LastName = "Nowak",
                Status = UserStatuses.Active,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(operatorUser, "SecureOperator2026!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(operatorUser, UserRoles.Operator);
            }
        }
    }
}