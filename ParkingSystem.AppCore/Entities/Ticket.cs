using System;

namespace ParkingSystem.AppCore.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    
    // Unikalny kod tekstowy biletu (kod kreskowy / QR)
    public string TicketCode { get; set; } = null!;
    
    // Dokładna data i godzina wjazdu na parking
    public DateTime IssuedAt { get; set; }
    
    // Powiązanie z bramką wjazdową
    public Guid EntryGateId { get; set; }
    
    // Czy bilet został opłacony w kasie / aplikacji
    public bool IsPaid { get; set; }
    
    // Dokładna data opłacenia (opcjonalna)
    public DateTime? PaidAt { get; set; }
}