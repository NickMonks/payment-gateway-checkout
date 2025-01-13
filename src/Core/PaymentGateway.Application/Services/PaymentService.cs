using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Contracts.Persistence;
using PaymentGateway.Application.Contracts.Services;
using PaymentGateway.Application.Helpers;
using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Shared.Mappers;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;

using ClientApiException = PaymentGateway.Application.Exceptions.ClientApiException;
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

            return mapper.Map<CreatePaymentResponseDto>(paymentResponse);
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

            return mapper.Map<CreatePaymentResponseDto>(rejectedEntity);
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