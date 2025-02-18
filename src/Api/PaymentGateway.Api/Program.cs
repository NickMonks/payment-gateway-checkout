using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using PaymentGateway.Api;
using PaymentGateway.Api.Middlewares;
using PaymentGateway.Api.Validators;
using PaymentGateway.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<PaymentsValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddLogging();
builder.Services.AddStartupServices(builder.Configuration);
builder.Services.AddObservability(builder.Configuration);

var app = builder.Build();

//Required to run migrations when the application starts up
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PaymentsDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, $"An error occurred while migrate the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionsMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<OpenTelemetryMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }