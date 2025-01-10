using AutoMapper;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.ApiClient.Models.Request;
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

    public PaymentService(
        ILogger<PaymentService> logger, 
        SimulatorApiClient simulatorApiClient, 
        IMapper mapper, 
        IPaymentsRepository paymentsRepository)
    {
        _logger = logger;
        _simulatorApiClient = simulatorApiClient;
        _mapper = mapper;
        _paymentsRepository = paymentsRepository;
    }
    
    public async Task<PostPaymentResponse> CreatePayment(PostPaymentRequest request)
    {
        var apiRequest = _mapper.Map<PostPaymentApiRequest>(request);
        var apiResponse = await _simulatorApiClient.CreatePaymentAsync(apiRequest);
        
        var paymentId = Guid.NewGuid();

        var postPaymentResponse = new PostPaymentResponse
        {
            Id = paymentId,
            Status = _mapper.Map<PaymentStatus>(apiResponse).ToString(),
            CardNumberLastFour = request.CardNumber.GetLastFourDigits(),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        var paymentEntity = postPaymentResponse.ToPayment(_mapper.Map<PaymentStatus>(apiResponse));
        await _paymentsRepository.CreatePaymentAsync(paymentEntity);

        return postPaymentResponse;
    }

    public async Task<GetPaymentResponse?> GetPayment(Guid paymentId)
    {
        var paymentDb = await _paymentsRepository.GetPaymentByIdAsync(paymentId);
        return paymentDb?.ToPayment();
    }
}