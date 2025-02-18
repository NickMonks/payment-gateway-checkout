using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Contracts.Persistence;
using PaymentGateway.Application.Contracts.Services;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Shared.Helpers;
using PaymentGateway.Shared.Mappers;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;
using PaymentGateway.Shared.Observability;

using IApiClient = PaymentGateway.Application.Contracts.ApiClient.IApiClient;

namespace PaymentGateway.Application.Services;

public class PaymentService(
    ILogger<PaymentService> logger,
    IApiClient simulatorApiClient,
    IMapper mapper,
    IPaymentsRepository paymentsRepository,
    IMemoryCache cache)
    : IPaymentService
{
    public async Task<CreatePaymentResponseDto> CreatePayment(CreatePaymentRequestDto request)
    {
        using var activity = DiagnosticsConfig.Source.StartActivity($"{nameof(PaymentService)}.{nameof(CreatePayment)}");
        var apiRequest = mapper.Map<PostPaymentApiRequest>(request);
        var paymentId = Guid.NewGuid();

        try
        {
            var apiResponse = await simulatorApiClient.CreatePaymentAsync(apiRequest);
            var paymentResponse = new PostPaymentResponse
            {
                Id = paymentId,
                Status = mapper.Map<PaymentStatus>(apiResponse).ToString(),
                CardNumberLastFour = request.CardNumber.GetLastFourDigits(),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            };

            var paymentEntity = paymentResponse.ToPayment(mapper.Map<PaymentStatus>(apiResponse));
            await paymentsRepository.CreatePaymentAsync(paymentEntity);

            return mapper.Map<CreatePaymentResponseDto>(paymentResponse);
        }
        catch (PaymentRejectedException ex)
        {
            logger.LogWarning(ex, "Payment rejected due to client error.");
            activity?.ClientApiExceptionEvent(paymentId.ToString());

            var rejectedPayment = new PostPaymentResponse
            {
                Id = paymentId,
                Status = PaymentStatus.Rejected.ToString(),
                CardNumberLastFour = request.CardNumber.GetLastFourDigits(),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            };

            var rejectedEntity = rejectedPayment.ToPayment(PaymentStatus.Rejected);
            await paymentsRepository.CreatePaymentAsync(rejectedEntity);

            return mapper.Map<CreatePaymentResponseDto>(rejectedPayment);
        }
    }

    public async Task<GetPaymentResponse?> GetPayment(Guid paymentId)
    {
        using var activity = DiagnosticsConfig.Source.StartActivity($"{nameof(PaymentService)}.{nameof(GetPayment)}");

        if (cache.TryGetValue(paymentId, out GetPaymentResponse? cachedPayment))
        {
            activity?.CacheEvent(paymentId.ToString(), true);
            logger.LogInformation($"Payment with ID {paymentId} retrieved from cache.");
            return cachedPayment;
        }

        activity?.CacheEvent(paymentId.ToString(), false);
        logger.LogInformation($"Payment with ID {paymentId} not found in cache. Retrieving from database.");
        var paymentDb = await paymentsRepository.GetPaymentByIdAsync(paymentId);

        if (paymentDb == null)
        {
            return null;
        }

        var paymentResponse = paymentDb.TogGetPaymentResponse();

        cache.Set(paymentId, paymentResponse, TimeSpan.FromMinutes(10));

        return paymentResponse;
    }
}