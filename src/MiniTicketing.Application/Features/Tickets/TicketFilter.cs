namespace MiniTicketing.Application.Features.Tickets;

public sealed record TicketFilter(int? Status = null, int? Priority = null, Guid? ReporterId = null, string? Search = null);