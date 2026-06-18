namespace SnapSharp.Domain.Exceptions;

public class SnapSharpException : Exception
{
    public string? ResponseCode { get; }
    public string? ResponseMessage { get; }

    public SnapSharpException(string message) : base(message) { }
    public SnapSharpException(string message, Exception innerException) : base(message, innerException) { }
    public SnapSharpException(string responseCode, string responseMessage)
        : base($"[{responseCode}] {responseMessage}")
    { ResponseCode = responseCode; ResponseMessage = responseMessage; }
}

public class SnapSharpAuthenticationException : SnapSharpException
{
    public SnapSharpAuthenticationException(string message) : base(message) { }
    public SnapSharpAuthenticationException(string responseCode, string responseMessage)
        : base(responseCode, responseMessage) { }
}

public class SnapSharpApiException : SnapSharpException
{
    public int HttpStatusCode { get; }
    public SnapSharpApiException(int httpStatusCode, string responseCode, string responseMessage)
        : base(responseCode, responseMessage) { HttpStatusCode = httpStatusCode; }
}

public class SnapSharpSignatureException : SnapSharpException
{
    public SnapSharpSignatureException(string message, Exception innerException)
        : base(message, innerException) { }
}
