using AutoMapper;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly SimulatorApiClient _simulatorApiClient;
    private readonly IMapper _mapper;

    public PaymentService(
        ILogger<PaymentService> logger, 
        SimulatorApiClient simulatorApiClient, 
        IMapper mapper)
    {
        _logger = logger;
        _simulatorApiClient = simulatorApiClient;
        _mapper = mapper;
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

        // TODO: Store postPaymentResponse in the database

        return postPaymentResponse;
    }

    public async Task<GetPaymentResponse> GetPayment(Guid paymentId)
    {
        throw new NotImplementedException();
    }
}