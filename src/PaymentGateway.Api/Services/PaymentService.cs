using AutoMapper;

using Microsoft.Extensions.Caching.Memory;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Contracts;

namespace PaymentGateway.Api.Services;

public class PaymentService(
    ILogger<PaymentService> logger,
    IApiClient simulatorApiClient,
    IMapper mapper,
    IPaymentsRepository paymentsRepository,
    IMemoryCache cache)
    : IPaymentService
{
    public async Task<PostPaymentResponse> CreatePayment(PostPaymentRequest request)
    {
        var apiRequest = mapper.Map<PostPaymentApiRequest>(request);
        var paymentId = Guid.NewGuid();

        try
        {
            logger.LogInformation($"Creating payment with id {paymentId}");
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

            return paymentResponse;
        }
        catch (ClientApiException ex)
        {
            // If there is a client exception type, we consider no payment could be created as invalid information
            // was supplied to the payment gateway, and therefore it has rejected the request without calling
            // the acquiring bank. Therefore, it will be stored and returned as Rejected. 
            logger.LogWarning(ex, "Payment rejected due to client error.");

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

            return rejectedPayment;
        }
    }

    public async Task<GetPaymentResponse?> GetPayment(Guid paymentId)
    {
        if (cache.TryGetValue(paymentId, out GetPaymentResponse? cachedPayment))
        {
            logger.LogInformation($"Payment with ID {paymentId} retrieved from cache.");
            return cachedPayment;
        }

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