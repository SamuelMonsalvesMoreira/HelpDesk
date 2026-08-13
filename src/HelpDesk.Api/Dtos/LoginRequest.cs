using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.Dtos;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(160)]
    public required string Email { get; set; }

    [Required, MinLength(8), MaxLength(100)]
    public required string Password { get; set; }
}
