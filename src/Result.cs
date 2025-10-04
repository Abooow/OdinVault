namespace OdinVault;

public interface IResult
{
    bool Succeeded { get; }
    string? Message { get; }
}

public interface IResult<out T> : IResult
{
    T? Data { get; }
}

public record Result(bool Succeeded, string? Message) : IResult
{
    private static readonly Result successResult = new Result(true, null);
    private static readonly Result failResult = new Result(false, null);
    
    public static Result Success()
    {
        return successResult;
    }

    public static Result Success(string message)
    {
        return successResult with { Message = message };
    }

    public static Result Fail()
    {
        return failResult;
    }

    public static Result Fail(string message)
    {
        return failResult with { Message = message };
    }
    
    public static Result CopyOf(Result copy)
    {
        return new Result(copy.Succeeded, copy.Message);
    }
}

public sealed record Result<T>(bool Succeeded, string? Message, T? Data)
    : Result(Succeeded, Message), IResult<T>
{
    private static readonly Result<T> successResult = new Result<T>(true, null, default);

    private static readonly Result<T> failResult = new Result<T>(false, null, default);

    public static new Result<T> Success()
    {
        return successResult;
    }

    public static Result<T> Success(T data)
    {
        return successResult with { Data = data };
    }

    public static new Result<T> Success(string message)
    {
        return successResult with { Message = message };
    }

    public static Result<T> Success(T data, string message)
    {
        return successResult with { Data = data, Message = message };
    }

    public static new Result<T> Fail()
    {
        return failResult;
    }

    public static Result<T> Fail(T data)
    {
        return failResult with { Data = data };
    }
    
    public static Result<T> Fail(T data, string message)
    {
        return failResult with { Data = data, Message = message };
    }

    public static new Result<T> Fail(string message)
    {
        return failResult with { Message = message };
    }
    
    public static Result<T> CopyOf(Result copy, T? data = default)
    {
        return new Result<T>(copy.Succeeded, copy.Message, data);
    }

    public static implicit operator Result<T>(T data)
    {
        return Success(data);
    }
}