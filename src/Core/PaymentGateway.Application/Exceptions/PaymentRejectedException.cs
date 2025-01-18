using System.Net;

namespace PaymentGateway.Application.Exceptions;

public class PaymentRejectedException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}