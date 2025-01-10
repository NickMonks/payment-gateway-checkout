using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        PaymentsRepository paymentsRepository, 
        IPaymentService paymentService, 
        ILogger<PaymentsController> logger)
    {
        _paymentsRepository = paymentsRepository;
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        return new OkObjectResult(payment);
    }

    [HttpPost]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostPaymentResponse?>> CreatePaymentAsync([FromBody] PostPaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var paymentResponse = await _paymentService.CreatePayment(request);
        return Ok(paymentResponse);
    }
}