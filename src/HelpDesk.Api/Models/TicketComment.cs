namespace HelpDesk.Api.Models;

public class TicketComment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int AuthorUserId { get; set; }
    public required string AuthorName { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
