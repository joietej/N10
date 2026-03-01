namespace N10.Services.Results;

public class Result<T>
{
    private readonly T? _value;
    private readonly List<Error>? _errors;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(List<Error> errors)
    {
        _errors = errors;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public List<Error> Errors => IsFailure
        ? _errors!
        : throw new InvalidOperationException("Cannot access Errors on a successful result.");

    public Error FirstError => Errors[0];

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new([error]);
    public static Result<T> Failure(List<Error> errors) => new(errors);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<List<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_errors!);

    public Result<TNew> Map<TNew>(Func<T, TNew> transform)
        => IsSuccess ? Result<TNew>.Success(transform(_value!)) : Result<TNew>.Failure(_errors!);

    public Result<TNew> Then<TNew>(Func<T, Result<TNew>> next)
        => IsSuccess ? next(_value!) : Result<TNew>.Failure(_errors!);

}

public class Result
{
    private readonly List<Error>? _errors;

    private Result() => IsSuccess = true;
    private Result(List<Error> errors)
    {
        _errors = errors;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public List<Error> Errors => IsFailure
        ? _errors!
        : throw new InvalidOperationException("Cannot access Errors on a successful result.");

    public Error FirstError => Errors[0];

    public static Result Success() => new();
    public static Result Failure(Error error) => new([error]);
    public static Result Failure(List<Error> errors) => new(errors);

    public static implicit operator Result(Error error) => Failure(error);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<List<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(_errors!);
}
