namespace HelpDesk.Api.Models;

public class TicketHistoryEntry
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public required string Action { get; set; }
    public required string Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
