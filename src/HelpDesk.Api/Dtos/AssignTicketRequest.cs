using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.Dtos;

public class AssignTicketRequest
{
    [Range(1, int.MaxValue)]
    public int TechnicianUserId { get; set; }
}
