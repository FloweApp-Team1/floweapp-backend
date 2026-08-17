namespace OrdersService.Domain.Enums
{
    // Only trusted once confirmed by the payment gateway webhook; never set client-side.
    public enum PaymentStatusEnum
    {
        Pending,
        Paid,
        Failed
    }
}
