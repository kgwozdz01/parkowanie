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

    public DbSet<ParkingGate> ParkingGates => Set<ParkingGate>();
    public DbSet<GateCameraImage> GateCameraImages => Set<GateCameraImage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Bardzo ważne: Wywołanie metody bazowej Identity, aby poprawnie skonfigurować tabele użytkowników i ról
        base.OnModelCreating(builder);

        // Konfiguracja relacji dla ParkingGate -> GateCameraImage (Jeden-do-wielu)
        builder.Entity<ParkingGate>()
            .HasMany(g => g.CameraImages)
            .WithOne(i => i.ParkingGate)
            .HasForeignKey(i => i.ParkingGateId)
            .OnDelete(DeleteBehavior.Cascade); // Usunięcie bramki usuwa powiązane zdjęcia

        // Konfiguracja relacji dla ApplicationUser -> RefreshToken (Jeden-do-wielu)
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(t => t.ApplicationUser)
            .HasForeignKey(t => t.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}