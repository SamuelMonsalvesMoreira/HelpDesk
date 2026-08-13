using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Dtos;

public class CreateTicketRequest
{
    [Required, MaxLength(120)]
    public required string Title { get; set; }

    [Required, MaxLength(2000)]
    public required string Description { get; set; }

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}
