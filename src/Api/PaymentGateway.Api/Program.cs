using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Handlers;
using PaymentGateway.Api.Middlewares;
using PaymentGateway.Api.Persistence;
using PaymentGateway.Api.Settings;
using PaymentGateway.Api.Validators;
using PaymentGateway.Application.Contracts.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<PaymentsValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<ExceptionsMiddleware>();
app.MapControllers();

app.Run();

public partial class Program { }