using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Api.Data;

public sealed class DemoDataSeeder(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("SeedDemoUsers"))
        {
            return;
        }

        var users = await userRepository.GetAllAsync(cancellationToken);

        if (users.Count > 0)
        {
            return;
        }

        await AddUserAsync("Administrador Demo", "admin@helpdesk.local", "Admin@123", UserRole.Administrator, cancellationToken);
        await AddUserAsync("Técnico Demo", "tecnico@helpdesk.local", "Tecnico@123", UserRole.Technician, cancellationToken);
        await AddUserAsync("Solicitante Demo", "usuario@helpdesk.local", "Usuario@123", UserRole.Requester, cancellationToken);
    }

    private async Task AddUserAsync(
        string name,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = string.Empty,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        await userRepository.AddAsync(user, cancellationToken);
    }
}
