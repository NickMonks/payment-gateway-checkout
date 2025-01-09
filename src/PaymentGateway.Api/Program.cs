using FluentValidation;
using FluentValidation.AspNetCore;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddValidatorsFromAssemblyContaining<PaymentsValidator>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

builder.Services.AddHttpClient(nameof(SimulatorApiClient));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
