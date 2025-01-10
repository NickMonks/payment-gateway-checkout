using System.Text.Json;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Diagnostics;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Settings;
using PaymentGateway.Api.Utility;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddValidatorsFromAssemblyContaining<PaymentsValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddLogging();
// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient<SimulatorApiClient>(nameof(SimulatorApiClient), client =>
{
    var simulatorApiSettings = builder.Configuration
        .GetSection(nameof(SimulatorApiSettings))
        .Get<SimulatorApiSettings>() ?? throw new NullReferenceException();
    
    client.BaseAddress = new Uri(simulatorApiSettings.BaseUri);
});



var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
//app.UseExceptionHandler();
app.UseMiddleware<ExceptionsMiddleware>();
app.MapControllers();

app.Run();
