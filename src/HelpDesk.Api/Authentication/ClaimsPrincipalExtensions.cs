using System.Security.Claims;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("O identificador do usuário não está disponível."));

    public static string GetUserName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Name)
            ?? throw new InvalidOperationException("O nome do usuário não está disponível.");

    public static string GetUserEmail(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
            ?? throw new InvalidOperationException("O e-mail do usuário não está disponível.");

    public static UserRole GetUserRole(this ClaimsPrincipal principal) =>
        Enum.Parse<UserRole>(principal.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("O perfil do usuário não está disponível."));
}
