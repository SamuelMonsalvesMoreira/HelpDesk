using HelpDesk.Api.Data;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Repositories;

public sealed class EfTicketRepository(HelpDeskDbContext dbContext) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(
        TicketStatus? status,
        TicketPriority? priority,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.Comments)
            .Include(ticket => ticket.History)
            .AsSplitQuery();

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
                ticket.Title.Contains(term)
                || ticket.Description.Contains(term)
                || ticket.RequesterName.Contains(term)
                || ticket.RequesterEmail.Contains(term));
        }

        return await query
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.Comments)
            .Include(ticket => ticket.History)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);

    public async Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public async Task<bool> UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        var current = await dbContext.Tickets
            .Include(item => item.Comments)
            .Include(item => item.History)
            .FirstOrDefaultAsync(item => item.Id == ticket.Id, cancellationToken);

        if (current is null)
        {
            return false;
        }

        current.Title = ticket.Title;
        current.Description = ticket.Description;
        current.RequesterName = ticket.RequesterName;
        current.RequesterEmail = ticket.RequesterEmail;
        current.Priority = ticket.Priority;
        current.Status = ticket.Status;
        current.UpdatedAtUtc = ticket.UpdatedAtUtc;

        foreach (var comment in ticket.Comments.Where(comment => comment.Id == 0))
        {
            current.Comments.Add(comment);
        }

        foreach (var entry in ticket.History.Where(entry => entry.Id == 0))
        {
            current.History.Add(entry);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.FindAsync([id], cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        dbContext.Tickets.Remove(ticket);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
