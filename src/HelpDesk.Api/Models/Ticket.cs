namespace HelpDesk.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string RequesterName { get; set; }
    public required string RequesterEmail { get; set; }
    public int? RequesterUserId { get; set; }
    public int? AssignedTechnicianUserId { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public List<TicketComment> Comments { get; set; } = [];
    public List<TicketHistoryEntry> History { get; set; } = [];
}
