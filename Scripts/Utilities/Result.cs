using System;

/// <summary>
/// نمط Result للتعامل الآمن مع النتائج والأخطاء
/// </summary>
public class Result
{
    public bool Success { get; protected set; }
    public string Message { get; protected set; }
    public Exception Exception { get; protected set; }

    protected Result(bool success, string message, Exception exception = null)
    {
        Success = success;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    /// نتيجة ناجحة
    /// </summary>
    public static Result Ok(string message = "Success")
    {
        return new Result(true, message);
    }

    /// <summary>
    /// نتيجة فاشلة
    /// </summary>
    public static Result Fail(string message, Exception ex = null)
    {
        return new Result(false, message, ex);
    }

    public override string ToString()
    {
        return $"[{(Success ? "✓" : "✗")}] {Message}";
    }
}

/// <summary>
/// Result مع بيانات
/// </summary>
public class Result<T> : Result
{
    public T Data { get; set; }

    private Result(bool success, string message, T data = default, Exception exception = null)
        : base(success, message, exception)
    {
        Data = data;
    }

    /// <summary>
    /// نتيجة ناجحة مع بيانات
    /// </summary>
    public static Result<T> Ok(T data, string message = "Success")
    {
        return new Result<T>(true, message, data);
    }

    /// <summary>
    /// نتيجة فاشلة
    /// </summary>
    public new static Result<T> Fail(string message, Exception ex = null)
    {
        return new Result<T>(false, message, default, ex);
    }
}

/// <summary>
/// استثناء مخصص للشبكة
/// </summary>
public class NetworkException : Exception
{
    public NetworkException(string message) : base(message) { }
    public NetworkException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// استثناء مخصص للمصادقة
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// استثناء مخصص لقاعدة البيانات
/// </summary>
public class DataException : Exception
{
    public DataException(string message) : base(message) { }
    public DataException(string message, Exception innerException) 
        : base(message, innerException) { }
}
