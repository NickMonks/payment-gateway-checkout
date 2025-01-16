namespace PaymentGateway.Shared.Models.Controller.Responses;

public class GetPaymentResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public int CardNumberLastFour { get; set; }
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}