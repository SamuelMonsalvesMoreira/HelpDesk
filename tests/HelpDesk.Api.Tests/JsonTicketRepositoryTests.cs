using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Api.Tests;

public sealed class JsonTicketRepositoryTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        "HelpDesk.Api.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task AddAsync_PersistsTicketForANewRepositoryInstance()
    {
        var firstRepository = CreateRepository();
        var created = await firstRepository.AddAsync(CreateTicket("Impressora offline"), CancellationToken.None);

        var secondRepository = CreateRepository();
        var persisted = await secondRepository.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal("Impressora offline", persisted.Title);
        Assert.Equal(1, persisted.Id);
    }

    [Fact]
    public async Task GetAllAsync_CombinesSearchStatusAndPriorityFilters()
    {
        var repository = CreateRepository();
        await repository.AddAsync(CreateTicket("Impressora do financeiro", TicketPriority.High), CancellationToken.None);

        var resolved = CreateTicket("Impressora da recepção", TicketPriority.High);
        resolved.Status = TicketStatus.Resolved;
        await repository.AddAsync(resolved, CancellationToken.None);

        await repository.AddAsync(CreateTicket("Mouse com defeito", TicketPriority.Low), CancellationToken.None);

        var result = await repository.GetAllAsync(
            TicketStatus.Open,
            TicketPriority.High,
            "financeiro",
            CancellationToken.None);

        var ticket = Assert.Single(result);
        Assert.Equal("Impressora do financeiro", ticket.Title);
    }

    [Fact]
    public async Task UpdateAsync_AssignsIdsToCommentsAndHistory()
    {
        var repository = CreateRepository();
        var ticket = await repository.AddAsync(CreateTicket("Acesso ao sistema"), CancellationToken.None);
        ticket.Comments.Add(new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = 2,
            AuthorName = "Técnico",
            Message = "Verificando o acesso."
        });
        ticket.History.Add(new TicketHistoryEntry
        {
            TicketId = ticket.Id,
            UserId = 2,
            UserName = "Técnico",
            Action = "Commented",
            Details = "Comentário adicionado."
        });

        var updated = await repository.UpdateAsync(ticket, CancellationToken.None);
        var persisted = await CreateRepository().GetByIdAsync(ticket.Id, CancellationToken.None);

        Assert.True(updated);
        Assert.NotNull(persisted);
        Assert.True(Assert.Single(persisted.Comments).Id > 0);
        Assert.True(Assert.Single(persisted.History).Id > 0);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private JsonTicketRepository CreateRepository()
    {
        Directory.CreateDirectory(_testDirectory);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:JsonFilePath"] = Path.Combine(_testDirectory, "tickets.json")
            })
            .Build();

        return new JsonTicketRepository(configuration, new TestWebHostEnvironment(_testDirectory));
    }

    private static Ticket CreateTicket(
        string title,
        TicketPriority priority = TicketPriority.Medium) =>
        new()
        {
            Title = title,
            Description = "Descrição do chamado para teste.",
            RequesterName = "Usuário Teste",
            RequesterEmail = "usuario@teste.local",
            RequesterUserId = 3,
            Priority = priority
        };
}
