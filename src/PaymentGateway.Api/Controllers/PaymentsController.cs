﻿using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Persistence.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.Contracts;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        IPaymentService paymentService, 
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentResponse = await _paymentService.GetPayment(id);
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

        var paymentResponse = await _paymentService.CreatePayment(request);
        return Ok(paymentResponse);
    }
}