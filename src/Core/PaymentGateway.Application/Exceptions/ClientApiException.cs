using System.Net;

namespace PaymentGateway.Application.Exceptions;

public class ClientApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}