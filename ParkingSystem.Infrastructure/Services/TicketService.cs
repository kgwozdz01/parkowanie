using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.Infrastructure.Persistence;

namespace ParkingSystem.Infrastructure.Services;

public class TicketService
{
    private readonly ParkingSystemDbContext _context;

    public TicketService(ParkingSystemDbContext context)
    {
        _context = context;
    }

    // =========================================================================
    // REJESTRACJA WJAZDU (Pobranie biletu)
    // =========================================================================
    public async Task<Ticket> IssueTicketAsync(Guid gateId)
    {
        var gate = await _context.Gates.FindAsync(gateId);
        if (gate == null || gate.Type != GateType.Entry)
            throw new ArgumentException("Nieprawidłowa bramka wjazdowa.");

        if (!gate.IsOperational)
            throw new InvalidOperationException("Bramka wjazdowa jest obecnie wyłączona z użytku (awaria).");

        // Tworzymy nowy bilet z unikalnym kodem do skanowania
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketCode = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            IssuedAt = DateTime.UtcNow,
            EntryGateId = gateId,
            IsPaid = false
        };

        _context.Set<Ticket>().Add(ticket);
        await _context.SaveChangesAsync();

        return ticket;
    }

    // =========================================================================
    // KALKULACJA OPŁATY PRZY WYJEŹDZIE
    // =========================================================================
    public async Task<decimal> CalculateParkingFeeAsync(string ticketCode)
    {
        var ticket = await _context.Set<Ticket>()
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);

        if (ticket == null)
            throw new ArgumentException("Nie znaleziono biletu o podanym kodzie.");

        // Pobieramy aktualnie aktywną taryfę standardową
        var activeTariff = await _context.Tariffs
            .FirstOrDefaultAsync(t => t.IsActive && t.Type == TariffType.Standard);

        if (activeTariff == null)
            throw new InvalidOperationException("Brak aktywnej taryfy miejskiej w systemie. Skontaktuj się z administratorem.");

        var duration = DateTime.UtcNow - ticket.IssuedAt;
        double totalMinutes = duration.TotalMinutes;

        // 1. Jeśli czas mieści się w darmowych minutach (np. 15 min), opłata wynosi 0 zł
        if (totalMinutes <= activeTariff.FreeMinutes)
        {
            return 0.00m;
        }

        // 2. Obliczamy czas płatny (po odliczeniu darmowych minut)
        double billableMinutes = totalMinutes - activeTariff.FreeMinutes;
        
        // Zaokrąglamy w górę do pełnych godzin (każda rozpoczęta godzina jest płatna)
        int billableHours = (int)Math.Ceiling(billableMinutes / 60.0);

        // 3. Naliczanie stawki godzinowej
        decimal calculatedFee = billableHours * activeTariff.HourlyRate;

        // 4. Sprawdzenie limitu dobowego (opłata nie może przekroczyć MaxDailyRate w ciągu 24h)
        if (calculatedFee > activeTariff.MaxDailyRate)
        {
            calculatedFee = activeTariff.MaxDailyRate;
        }

        return calculatedFee;
    }
}