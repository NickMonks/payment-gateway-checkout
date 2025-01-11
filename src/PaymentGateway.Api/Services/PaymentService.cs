using AutoMapper;

using Microsoft.Extensions.Caching.Memory;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Contracts;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly SimulatorApiClient _simulatorApiClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public PaymentService(
        ILogger<PaymentService> logger, 
        SimulatorApiClient simulatorApiClient, 
        IMapper mapper, 
        IPaymentsRepository paymentsRepository, 
        IMemoryCache cache)
    {
        _logger = logger;
        _simulatorApiClient = simulatorApiClient;
        _mapper = mapper;
        _paymentsRepository = paymentsRepository;
        _cache = cache;
    }
    
    public async Task<PostPaymentResponse> CreatePayment(PostPaymentRequest request)
    {
        var apiRequest = _mapper.Map<PostPaymentApiRequest>(request);
        var paymentId = Guid.NewGuid();

        try
        {
            _logger.LogInformation($"Creating payment with id {paymentId}");
            var apiResponse = await _simulatorApiClient.CreatePaymentAsync(apiRequest);
            var paymentResponse = new PostPaymentResponse
            {
                Id = paymentId,
                Status = _mapper.Map<PaymentStatus>(apiResponse).ToString(),
                CardNumberLastFour = request.CardNumber.GetLastFourDigits(),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            };

            var paymentEntity = paymentResponse.ToPayment(_mapper.Map<PaymentStatus>(apiResponse));
            await _paymentsRepository.CreatePaymentAsync(paymentEntity);

            return paymentResponse;
        }
        catch (ClientApiException ex)
        {
            // If there is a client exception type, we consider no payment could be created as invalid information
            // was supplied to the payment gateway and therefore it has rejected the request without calling
            // the acquiring bank. Therefore, it will be stored and returned as Rejected. 
            _logger.LogError(ex, "Payment rejected due to client error.");

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
            await _paymentsRepository.CreatePaymentAsync(rejectedEntity);

            return rejectedPayment;
        }
    }

    public async Task<GetPaymentResponse?> GetPayment(Guid paymentId)
    {
        if (_cache.TryGetValue(paymentId, out GetPaymentResponse? cachedPayment))
        {
            _logger.LogInformation($"Payment with ID {paymentId} retrieved from cache.");
            return cachedPayment;
        }

        _logger.LogInformation($"Payment with ID {paymentId} not found in cache. Retrieving from database.");
        var paymentDb = await _paymentsRepository.GetPaymentByIdAsync(paymentId);

        if (paymentDb == null)
        {
            return null;
        }

        var paymentResponse = paymentDb.ToPayment();

        _cache.Set(paymentId, paymentResponse, TimeSpan.FromMinutes(10));

        return paymentResponse;
    }
}