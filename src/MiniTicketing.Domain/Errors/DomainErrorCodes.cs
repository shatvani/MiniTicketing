namespace MiniTicketing.Domain.Errors;

/// <summary>
/// Stabil, gép-barát hibaazonosítók. Ezek jelennek meg Result/ProblemDetails kimenetben.
/// Névkonvenció: "kategória.kód"
/// </summary>
public static class DomainErrorCodes
{
    public static class Common
    {
        public const string ValidationError = "common.validation_error";
        public const string NotFound        = "common.not_found";
        public const string Conflict        = "common.conflict";
    }

    public static class Ticket
    {
        public const string InvalidStatusTransition = "ticket.invalid_status_transition";
        public const string TitleTooShort           = "ticket.title_too_short";
        public const string DueDateInPast           = "ticket.due_date_in_past";
        public const string AssigneeRequired        = "ticket.assignee_required"; // ha később kell
    }

    public static class Label
    {
        public const string NameNotUnique = "label.name_not_unique";
        public const string NameInvalid   = "label.name_invalid"; // pl. üres, túl hosszú stb.
    }

    public static class Comment
    {
        public const string TextRequired = "comment.text_required";
    }
}
