using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Api.Tests;

public sealed class JsonUserRepositoryTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        "HelpDesk.Api.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task AddAsync_RejectsDuplicatedEmailIgnoringCase()
    {
        var repository = CreateRepository();
        await repository.AddAsync(CreateUser("usuario@helpdesk.local"), CancellationToken.None);

        var action = () => repository.AddAsync(CreateUser("USUARIO@helpdesk.local"), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("e-mail", exception.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private JsonUserRepository CreateRepository()
    {
        Directory.CreateDirectory(_testDirectory);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:JsonUsersFilePath"] = Path.Combine(_testDirectory, "users.json")
            })
            .Build();

        return new JsonUserRepository(configuration, new TestWebHostEnvironment(_testDirectory));
    }

    private static User CreateUser(string email) =>
        new()
        {
            Name = "Usuário Teste",
            Email = email,
            PasswordHash = "hash-de-teste",
            Role = UserRole.Requester
        };
}
