using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Infrastructure.Services;

namespace ParkingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriverController : ControllerBase
{
    private readonly TicketService _ticketService;

    public DriverController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // =========================================================================
    // POST: api/driver/enter
    // Symulacja podjazdu pod szlaban wjazdowy i pobrania biletu
    // =========================================================================
    [HttpPost("enter")]
    public async Task<IActionResult> EnterParking([FromBody] EnterParkingRequest request)
    {
        try
        {
            var ticket = await _ticketService.IssueTicketAsync(request.GateId);
            
            // Zwracamy status 200 OK wraz z danymi biletu (kodem i datą wjazdu)
            return Ok(new
            {
                Message = "Szlaban otwarty. Witamy na parkingu!",
                TicketCode = ticket.TicketCode,
                IssuedAt = ticket.IssuedAt
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { Error = ex.Message }); // 503 Service Unavailable (np. awaria bramki)
        }
    }

    // =========================================================================
    // GET: api/driver/calculate-fee
    // Sprawdzenie aktualnej kwoty do zapłaty na podstawie kodu biletu
    // =========================================================================
    [HttpGet("calculate-fee")]
    public async Task<IActionResult> GetParkingFee([FromQuery] string ticketCode)
    {
        try
        {
            var fee = await _ticketService.CalculateParkingFeeAsync(ticketCode);
            
            return Ok(new
            {
                TicketCode = ticketCode,
                CurrentFee = fee,
                Currency = "PLN",
                CalculatedAt = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}

// DTO dla żądania wjazdu
public class EnterParkingRequest
{
    public Guid GateId { get; set; }
}