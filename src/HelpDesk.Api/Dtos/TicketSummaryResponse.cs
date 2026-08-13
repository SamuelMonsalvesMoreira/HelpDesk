namespace HelpDesk.Api.Dtos;

public record TicketSummaryResponse(
    int Total,
    int Open,
    int InProgress,
    int Resolved,
    int Closed);
