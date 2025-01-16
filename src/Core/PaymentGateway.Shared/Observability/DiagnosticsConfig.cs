using System.Diagnostics;

using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Shared.Observability;

public static class DiagnosticsConfig
{
    public const string SourceName = "PaymentGateway.Api";
    public static readonly ActivitySource Source = new ActivitySource(SourceName);
}

public static class ActivityExtensions
{
    public static void SetPayment(this Activity activity, Payment payment)
    {
        activity?.SetTag("payment-id", payment.PaymentId);
        activity?.SetTag("payment-status", payment.PaymentStatus);
        activity?.SetTag("payment-amount", payment.Amount);
        activity?.SetTag("payment-currency", payment.Currency);
    }

    public static void CacheEvent(this Activity activity, string paymentId, bool isHit)
    {
        activity?.AddEvent(new ActivityEvent("CacheHit", default, new ActivityTagsCollection
        {
            { "payment.id", paymentId },
            { "cache.hit", isHit }
        }));
    }

    public static void ClientApiExceptionEvent(this Activity activity, string paymentId)
    {
        activity?.AddEvent(new ActivityEvent("ClientApiException", default, new ActivityTagsCollection
        {
            { "payment.id", paymentId },
        }));
    }
}