// MiniTicketing.Domain/Entities/Ticket.cs
using MiniTicketing.Domain.Common;
using MiniTicketing.Domain.Errors;
using MiniTicketing.Domain.Specifications;
using MiniTicketing.Domain.Enums;

namespace MiniTicketing.Domain.Entities;

public partial class Ticket
{
    public DomainResult CanApply(TicketStatus newStatus, DateTime? newDueDateUtc)
    {
        // 1) státusz átmenet
        if (this.Status != newStatus)
        {
            var spec = TicketStatusTransitionAllowed.IsSatisfiedBy(this.Status, newStatus);
            if (!spec.IsSatisfied)
                return DomainResult.Failure(spec.ErrorCode ?? DomainErrorCodes.Ticket.InvalidStatusTransition);
        }

        // 2) határidő
        if (newDueDateUtc is not null)
        {
            var due = DueDateNotPast.IsSatisfiedBy(newDueDateUtc.Value);
            if (!due.IsSatisfied)
                return DomainResult.Failure(due.ErrorCode ?? DomainErrorCodes.Ticket.DueDateInPast);
        }

        return DomainResult.Success();
    }
}
