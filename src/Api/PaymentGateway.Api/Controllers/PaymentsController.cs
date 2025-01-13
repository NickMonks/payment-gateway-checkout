using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Application.Contracts.Services;
using PaymentGateway.Shared.Models.Controller.Requests;
using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService, IMapper mapper) : Controller
{

    [HttpGet("{id:guid}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentResponse = await paymentService.GetPayment(id);
        if (paymentResponse == null)
        {
            return NotFound("Payment not found");
        }
        return Ok(paymentResponse);
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

        var paymentResponse = await paymentService.CreatePayment(
            mapper.Map<CreatePaymentRequestDto>(request)
            );
        return Ok(paymentResponse);
    }
}