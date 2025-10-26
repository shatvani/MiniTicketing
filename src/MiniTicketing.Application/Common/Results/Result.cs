namespace MiniTicketing.Application.Common.Results;

public readonly struct Result
{
    public bool Success { get; }
    public string? ErrorCode { get; }

    private Result(bool success, string? error) { Success = success; ErrorCode = error; }

    public static Result Ok() => new(true, null);
    public static Result Fail(string code) => new(false, code);
}

public readonly struct Result<T>
{
    public bool Success { get; }
    public string? ErrorCode { get; }
    public T? Value { get; }

    private Result(bool success, T? value, string? error) { Success = success; Value = value; ErrorCode = error; }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string code) => new(false, default, code);
}
