namespace Shared.Events.OrderEvents;

public sealed record DeliveryConfirmedByCustomerEvent(
    Guid OrderId,
    string OrderNumber,
    Guid DriverId,
    Guid CustomerId,
    DateTime ConfirmedAt);
