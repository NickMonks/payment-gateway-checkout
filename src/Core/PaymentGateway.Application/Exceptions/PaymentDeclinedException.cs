using System.Net;

namespace PaymentGateway.Application.Exceptions;

public class PaymentDeclinedException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}