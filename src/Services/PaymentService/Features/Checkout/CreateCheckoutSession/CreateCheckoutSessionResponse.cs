namespace PaymentService.Features.Checkout.CreateCheckoutSession
{
    public record CreateCheckoutSessionResponse(
        string CheckoutUrl,
        string StripeSessionId,
        Guid PaymentAttemptId,
        DateTime? ExpiresAt,   
        string? SuccessUrl,    
        string? CancelUrl       
    );
}
