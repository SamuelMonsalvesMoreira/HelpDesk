using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDesk.Api.Models;

namespace HelpDesk.Api.Repositories;

public sealed class JsonTicketRepository : ITicketRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonTicketRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration["Storage:JsonFilePath"] ?? "App_Data/tickets.json";
        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(
        TicketStatus? status,
        TicketPriority? priority,
        string? search,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            IEnumerable<Ticket> query = await ReadAllAsync(cancellationToken);

            if (status is not null)
            {
                query = query.Where(ticket => ticket.Status == status);
            }

            if (priority is not null)
            {
                query = query.Where(ticket => ticket.Priority == priority);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(ticket =>
                    ticket.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || ticket.Description.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || ticket.RequesterName.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || ticket.RequesterEmail.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(ticket => ticket.CreatedAtUtc)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var tickets = await ReadAllAsync(cancellationToken);
            return tickets.FirstOrDefault(ticket => ticket.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var tickets = await ReadAllAsync(cancellationToken);
            ticket.Id = tickets.Count == 0 ? 1 : tickets.Max(item => item.Id) + 1;
            AssignNestedIds(tickets, ticket);
            tickets.Add(ticket);
            await WriteAllAsync(tickets, cancellationToken);
            return ticket;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var tickets = await ReadAllAsync(cancellationToken);
            var index = tickets.FindIndex(item => item.Id == ticket.Id);

            if (index < 0)
            {
                return false;
            }

            AssignNestedIds(tickets, ticket);
            tickets[index] = ticket;
            await WriteAllAsync(tickets, cancellationToken);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var tickets = await ReadAllAsync(cancellationToken);
            var removed = tickets.RemoveAll(ticket => ticket.Id == id) > 0;

            if (removed)
            {
                await WriteAllAsync(tickets, cancellationToken);
            }

            return removed;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<Ticket>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<Ticket>>(stream, _jsonOptions, cancellationToken)
            ?? [];
    }

    private async Task WriteAllAsync(List<Ticket> tickets, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("Não foi possível determinar a pasta de dados.");

        Directory.CreateDirectory(directory);
        var temporaryPath = $"{_filePath}.tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, tickets, _jsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, _filePath, true);
    }

    private static void AssignNestedIds(List<Ticket> tickets, Ticket ticket)
    {
        var nextCommentId = tickets
            .SelectMany(item => item.Comments)
            .Select(comment => comment.Id)
            .DefaultIfEmpty(0)
            .Max() + 1;

        foreach (var comment in ticket.Comments.Where(comment => comment.Id == 0))
        {
            comment.Id = nextCommentId++;
            comment.TicketId = ticket.Id;
        }

        var nextHistoryId = tickets
            .SelectMany(item => item.History)
            .Select(entry => entry.Id)
            .DefaultIfEmpty(0)
            .Max() + 1;

        foreach (var entry in ticket.History.Where(entry => entry.Id == 0))
        {
            entry.Id = nextHistoryId++;
            entry.TicketId = ticket.Id;
        }
    }
}
