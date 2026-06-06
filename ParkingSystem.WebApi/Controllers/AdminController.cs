using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.AppCore.Dtos;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.Infrastructure.Persistence;

namespace ParkingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Pełna blokada bezpieczeństwa - tylko dla roli Admin
public class AdminController : ControllerBase
{
    private readonly ParkingSystemDbContext _context;

    public AdminController(ParkingSystemDbContext context)
    {
        _context = context;
    }

    // =========================================================================
    // SEKCJA: SYSTEM TARYFOWY (Czas darmowy, stawki, taryfy)
    // =========================================================================

    [HttpPost("tariffs")]
    public async Task<IActionResult> CreateTariff([FromBody] CreateTariffRequest request)
    {
        // Pobieramy identyfikator zalogowanego Admina z roszczeń (Claims) tokenu JWT
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var tariff = new Tariff
        {
            Name = request.Name,
            Type = request.Type,
            FreeMinutes = request.FreeMinutes,
            HourlyRate = request.HourlyRate,
            MaxDailyRate = request.MaxDailyRate,
            CreatedBy = adminId,
            IsActive = true
        };

        _context.Tariffs.Add(tariff);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTariffById), new { id = tariff.Id }, tariff);
    }

    [HttpPut("tariffs/{id:guid}")]
    public async Task<IActionResult> UpdateTariff(Guid id, [FromBody] UpdateTariffRequest request)
    {
        var tariff = await _context.Tariffs.FindAsync(id);
        if (tariff == null) return NotFound("Wskazana taryfa nie istnieje w systemie.");

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        tariff.Name = request.Name;
        tariff.FreeMinutes = request.FreeMinutes;
        tariff.HourlyRate = request.HourlyRate;
        tariff.MaxDailyRate = request.MaxDailyRate;
        tariff.UpdatedBy = adminId;
        tariff.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(tariff);
    }

    [HttpPatch("tariffs/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleTariffStatus(Guid id)
    {
        var tariff = await _context.Tariffs.FindAsync(id);
        if (tariff == null) return NotFound("Wskazana taryfa nie istnieje.");

        // Przełączanie stanu logicznego aktywacji/dezaktywacji
        tariff.IsActive = !tariff.IsActive;
        tariff.UpdatedAt = DateTime.UtcNow;
        tariff.UpdatedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Taryfa została {(tariff.IsActive ? "aktywowana" : "dezaktywowana")}.", currentStatus = tariff.IsActive });
    }

    [HttpGet("tariffs/{id:guid}")]
    public async Task<IActionResult> GetTariffById(Guid id)
    {
        var tariff = await _context.Tariffs.FindAsync(id);
        return tariff == null ? NotFound() : Ok(tariff);
    }

    [HttpGet("tariffs")]
    public async Task<IActionResult> GetAllTariffs()
    {
        var tariffs = await _context.Tariffs.ToListAsync();
        return Ok(tariffs);
    }

    // =========================================================================
    // SEKCJA: URZĄDZENIA BRZEGOWE (Rejestracja i sterowanie bramkami/szlabanami)
    // =========================================================================

    [HttpPost("gates")]
    public async Task<IActionResult> RegisterGate([FromBody] RegisterGateRequest request)
    {
        var gate = new Gate
        {
            Name = request.Name,
            Type = request.Type,
            Location = request.Location,
            IsOperational = true // Domyślnie nowo zarejestrowany szlaban działa
        };

        _context.Gates.Add(gate);
        await _context.SaveChangesAsync();

        return Ok(gate);
    }

    [HttpPatch("gates/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleGateOperational(Guid id)
    {
        var gate = await _context.Gates.FindAsync(id);
        if (gate == null) return NotFound("Wskazana bramka nie istnieje w bazie danych.");

        // Przełączanie trybu działania (np. odcięcie uszkodzonego szlabanu)
        gate.IsOperational = !gate.IsOperational;

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Bramka została {(gate.IsOperational ? "włączona" : "wyłączona (tryb awaryjny/blokada)")}.", isOperational = gate.IsOperational });
    }

    [HttpGet("gates")]
    public async Task<IActionResult> GetAllGates()
    {
        var gates = await _context.Gates.ToListAsync();
        return Ok(gates);
    }
}