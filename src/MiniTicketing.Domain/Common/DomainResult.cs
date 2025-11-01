namespace MiniTicketing.Domain.Common;

public sealed class DomainResult
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }

    private DomainResult(bool isSuccess, string? errorCode)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
    }

    public static DomainResult Success() => new(true, null);

    public static DomainResult Failure(string errorCode) => new(false, errorCode);
}
