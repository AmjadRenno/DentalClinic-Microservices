namespace DentalClinic.SharedKernel.Results;

/// <summary>
/// Represents the result of an operation that can either succeed or fail
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string ErrorCode { get; }

    protected Result(bool isSuccess, string error, string errorCode = "")
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error message.");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string error, string errorCode = "ERROR") 
        => new(false, error, errorCode);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    public static Result<T> Failure<T>(string error, string errorCode = "ERROR") 
        => new(default!, false, error, errorCode);
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error, string errorCode = "")
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }
}
