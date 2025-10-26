namespace MiniTicketing.Domain.Specifications;

/// <summary>
/// Egységes kimenet specifikációkhoz (nem dob kivételt, a hívó dönt).
/// </summary>
public readonly record struct SpecResult(bool IsSatisfied, string? ErrorCode)
{
  public static SpecResult Success() => new(true, null);
  public static SpecResult Failure(string errorCode) => new(false, errorCode);
}