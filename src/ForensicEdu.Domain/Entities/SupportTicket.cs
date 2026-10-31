using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class SupportTicket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TicketCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Message { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.ABIERTO;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ResponsesJson { get; set; } = "[]";
}
