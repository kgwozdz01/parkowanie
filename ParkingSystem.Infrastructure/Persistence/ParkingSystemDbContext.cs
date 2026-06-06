using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.AppCore.Entities; 

namespace ParkingSystem.Infrastructure.Persistence;

public class ParkingSystemDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ParkingSystemDbContext(DbContextOptions<ParkingSystemDbContext> options) : base(options)
    {
    }

    // Tabele systemowe i bezpieczeństwa
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Nowe tabele biznesowe dla systemu parkingowego
    public DbSet<Tariff> Tariffs { get; set; } = null!;
    public DbSet<Gate> Gates { get; set; } = null!;
    // Nowe tabele biznesowe dla systemu parkingowego
    public DbSet<Ticket> Tickets { get; set; } = null!; // <--- DOPISZ TĘ LINIĘ

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Konfiguracja konwersji typów dla bazy SQLite (brak natywnego decimal)
        builder.Entity<Tariff>()
            .Property(t => t.HourlyRate)
            .HasConversion<double>();

        builder.Entity<Tariff>()
            .Property(t => t.MaxDailyRate)
            .HasConversion<double>();
    }
}