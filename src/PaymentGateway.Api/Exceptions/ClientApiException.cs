using System.Net;

namespace PaymentGateway.Api.Exceptions;

public class ClientApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ClientApiException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}