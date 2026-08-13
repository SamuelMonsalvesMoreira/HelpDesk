using HelpDesk.Api.Authentication;
using HelpDesk.Api.Dtos;
using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(),
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        return Ok(tokenService.Create(user));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserResponse> GetCurrentUser() =>
        Ok(new UserResponse(
            User.GetUserId(),
            User.GetUserName(),
            User.GetUserEmail(),
            User.GetUserRole()));
}
