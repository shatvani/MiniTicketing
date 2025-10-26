using System;
using MiniTicketing.Domain.Errors;

namespace MiniTicketing.Domain.Specifications;

/// <summary>
/// Üzleti szabály: a DueDate nem lehet a múltban (UTC nap-pontosság).
/// Null (nincs határidő) = megfelel.
/// </summary>
public static class DueDateNotPast
{
    /// <summary>
    /// Kényelmi overload: a rendszer aktuális UTC idejével ellenőriz.
    /// </summary>
    public static SpecResult IsSatisfiedBy(DateTime? dueDateUtc)
        => IsSatisfiedBy(dueDateUtc, DateTime.UtcNow);

    /// <summary>
    /// Tesztelhető overload: explicit "most" idővel.
    /// </summary>
    public static SpecResult IsSatisfiedBy(DateTime? dueDateUtc, DateTime utcNow)
    {
        if (dueDateUtc is null)
            return SpecResult.Success();

        // Napra kerekítve ellenőrzünk (határidő a múlt napjára esik -> hiba)
        if (dueDateUtc.Value.Date >= utcNow.Date)
            return SpecResult.Success();

        return SpecResult.Failure(DomainErrorCodes.Ticket.DueDateInPast);
    }
}
