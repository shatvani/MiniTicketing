namespace MiniTicketing.Application.Features.Tickets.CreateTicket;

using FluentValidation;
using MiniTicketing.Domain.Enums;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
  public CreateTicketCommandValidator()
  {
    RuleFor(x => x.ticketDto.Title)
        .Cascade(CascadeMode.Stop)
        .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title must not be empty or whitespace.")
        .Must(title => (title?.Trim().Length ?? 0) <= 200)
            .WithMessage("Title must be at most 200 characters.");

    RuleFor(x => x.ticketDto.Priority)
      .Must(priority => Enum.IsDefined(typeof(PriorityLevel), priority))
      .WithMessage("Unknown priority value.");

    RuleFor(x => x.ticketDto.ReporterId)
      .NotEqual(Guid.Empty)
      .WithMessage("ReporterId must be a valid non-empty GUID.");
  }
}