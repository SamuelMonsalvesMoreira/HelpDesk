using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.Dtos;

public class AddCommentRequest
{
    [Required, MaxLength(1000)]
    public required string Message { get; set; }
}
