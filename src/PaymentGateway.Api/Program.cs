using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.Handlers;
using PaymentGateway.Api.Middlewares;
using PaymentGateway.Api.Persistence;
using PaymentGateway.Api.Persistence.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.Contracts;
using PaymentGateway.Api.Settings;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddValidatorsFromAssemblyContaining<PaymentsValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddLogging();
// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient<IApiClient, SimulatorApiClient>(nameof(SimulatorApiClient), client =>
    {
        var simulatorApiSettings = builder.Configuration
            .GetSection(nameof(SimulatorApiSettings))
            .Get<SimulatorApiSettings>() ?? throw new NullReferenceException();

        client.BaseAddress = new Uri(simulatorApiSettings.BaseUri);
    })
    .AddHttpMessageHandler<ApiExceptionHandler>();
builder.Services.AddTransient<ApiExceptionHandler>();


builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();
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