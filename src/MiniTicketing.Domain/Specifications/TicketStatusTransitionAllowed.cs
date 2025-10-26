// Domain/Specifications/TicketStatusTransitionAllowed.cs
using MiniTicketing.Domain.Enums;
using MiniTicketing.Domain.Errors;


namespace MiniTicketing.Domain.Specifications;

/// <summary>
/// Üzleti szabály: engedélyezett-e a státuszváltás a current → next pároson.
/// Perzisztencia-agnosztikus, tiszta logika.
/// Használat
/// var check = TicketStatusTransitionAllowed.IsSatisfiedBy(current, next);
/// if (!check.IsSatisfied) return Result.Fail(check.ErrorCode); (vagy dobj domain kivételt – a te stratégiád szerint).
/// </summary>
public static class TicketStatusTransitionAllowed
{
  // Engedélyezett átmenetek mátrixa
  private static readonly HashSet<(TicketStatus From, TicketStatus To)> Allowed = new()
    {
        (TicketStatus.New,        TicketStatus.InProgress),
        (TicketStatus.New,        TicketStatus.Closed),

        (TicketStatus.InProgress, TicketStatus.Resolved),
        (TicketStatus.InProgress, TicketStatus.Closed),

        (TicketStatus.Resolved,   TicketStatus.InProgress),
        (TicketStatus.Resolved,   TicketStatus.Closed),

        (TicketStatus.Closed,     TicketStatus.InProgress) // reopen
    };

  /// <summary>
  /// Igaz/hamis + hibaazonosítóval tér vissza. A current==next nem értelmes váltás: false.
  /// </summary>
  public static SpecResult IsSatisfiedBy(TicketStatus current, TicketStatus next)
  {
    if (current == next)
      return SpecResult.Failure(DomainErrorCodes.Ticket.InvalidStatusTransition);

    return Allowed.Contains((current, next))
        ? SpecResult.Success()
        : SpecResult.Failure(DomainErrorCodes.Ticket.InvalidStatusTransition);
  }
}

