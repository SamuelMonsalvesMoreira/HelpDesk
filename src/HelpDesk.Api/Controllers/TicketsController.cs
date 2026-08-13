using HelpDesk.Api.Authentication;
using HelpDesk.Api.Dtos;
using HelpDesk.Api.Models;
using HelpDesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController(
    ITicketRepository repository,
    IUserRepository userRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Ticket>>> GetAll(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] string? search,
        [FromQuery] bool assignedToMe,
        CancellationToken cancellationToken)
    {
        var tickets = await GetAccessibleTicketsAsync(status, priority, search, cancellationToken);

        if (assignedToMe && User.GetUserRole() != UserRole.Requester)
        {
            tickets = tickets
                .Where(ticket => ticket.AssignedTechnicianUserId == User.GetUserId())
                .ToList();
        }

        return Ok(tickets);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<TicketSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var tickets = await GetAccessibleTicketsAsync(null, null, null, cancellationToken);
        var summary = new TicketSummaryResponse(
            tickets.Count,
            tickets.Count(ticket => ticket.Status == TicketStatus.Open),
            tickets.Count(ticket => ticket.Status == TicketStatus.InProgress),
            tickets.Count(ticket => ticket.Status == TicketStatus.Resolved),
            tickets.Count(ticket => ticket.Status == TicketStatus.Closed));

        return Ok(summary);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetById(int id, CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return CanAccess(ticket) ? Ok(ticket) : Forbid();
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            RequesterName = User.GetUserName(),
            RequesterEmail = User.GetUserEmail(),
            RequesterUserId = User.GetUserId(),
            Priority = request.Priority
        };

        ticket.History.Add(CreateHistoryEntry(
            ticket,
            "Created",
            $"Chamado criado com prioridade {ticket.Priority}."));

        await repository.AddAsync(ticket, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [Authorize(Roles = "Technician,Administrator")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        var changes = new List<string>();

        if (ticket.Status != request.Status)
        {
            changes.Add($"status: {ticket.Status} → {request.Status}");
        }

        if (ticket.Priority != request.Priority)
        {
            changes.Add($"prioridade: {ticket.Priority} → {request.Priority}");
        }

        if (!ticket.Title.Equals(request.Title.Trim(), StringComparison.Ordinal))
        {
            changes.Add("título alterado");
        }

        if (!ticket.Description.Equals(request.Description.Trim(), StringComparison.Ordinal))
        {
            changes.Add("descrição alterada");
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Priority = request.Priority;
        ticket.Status = request.Status;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        if (changes.Count > 0)
        {
            ticket.History.Add(CreateHistoryEntry(ticket, "Updated", string.Join("; ", changes)));
        }

        var updated = await repository.UpdateAsync(ticket, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Technician,Administrator")]
    [HttpPatch("{id:int}/assign")]
    public async Task<IActionResult> Assign(
        int id,
        AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        var technician = await userRepository.GetByIdAsync(request.TechnicianUserId, cancellationToken);

        if (technician is null || technician.Role != UserRole.Technician || !technician.IsActive)
        {
            return BadRequest(new { message = "O técnico informado não é válido." });
        }

        ticket.AssignedTechnicianUserId = technician.Id;
        ticket.AssignedTechnicianName = technician.Name;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        ticket.History.Add(CreateHistoryEntry(
            ticket,
            "Assigned",
            $"Chamado atribuído a {technician.Name}."));

        await repository.UpdateAsync(ticket, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketComment>> AddComment(
        int id,
        AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.GetByIdAsync(id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        if (!CanAccess(ticket))
        {
            return Forbid();
        }

        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = User.GetUserId(),
            AuthorName = User.GetUserName(),
            Message = request.Message.Trim()
        };

        ticket.Comments.Add(comment);
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        ticket.History.Add(CreateHistoryEntry(ticket, "Commented", "Novo comentário adicionado."));
        await repository.UpdateAsync(ticket, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, comment);
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private async Task<IReadOnlyList<Ticket>> GetAccessibleTicketsAsync(
        TicketStatus? status,
        TicketPriority? priority,
        string? search,
        CancellationToken cancellationToken)
    {
        var tickets = await repository.GetAllAsync(status, priority, search, cancellationToken);

        if (User.GetUserRole() != UserRole.Requester)
        {
            return tickets;
        }

        var userId = User.GetUserId();
        var email = User.GetUserEmail();
        return tickets
            .Where(ticket => ticket.RequesterUserId == userId
                || ticket.RequesterEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private bool CanAccess(Ticket ticket) =>
        User.GetUserRole() != UserRole.Requester
        || ticket.RequesterUserId == User.GetUserId()
        || ticket.RequesterEmail.Equals(User.GetUserEmail(), StringComparison.OrdinalIgnoreCase);

    private TicketHistoryEntry CreateHistoryEntry(Ticket ticket, string action, string details) =>
        new()
        {
            TicketId = ticket.Id,
            UserId = User.GetUserId(),
            UserName = User.GetUserName(),
            Action = action,
            Details = details
        };
}
