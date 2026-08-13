using HelpDesk.Api.Dtos;
using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = "Technician,Administrator")]
[Route("api/users")]
public class UsersController(IUserRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        [FromQuery] UserRole? role,
        CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        var result = users
            .Where(user => user.IsActive && (role is null || user.Role == role))
            .Select(user => new UserResponse(user.Id, user.Name, user.Email, user.Role))
            .ToList();

        return Ok(result);
    }
}
