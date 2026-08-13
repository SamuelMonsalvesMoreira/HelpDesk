using HelpDesk.Api.Models;

namespace HelpDesk.Api.Repositories;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetAllAsync(
        TicketStatus? status,
        TicketPriority? priority,
        string? search,
        CancellationToken cancellationToken);

    Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
