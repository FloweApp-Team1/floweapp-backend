using MediatR;
using OrdersService.Domain.Enums;
using Shared.Results;

namespace OrdersService.Features.Checkout.PlaceOrder
{
   
      
        public sealed record PlaceOrderCommand(
            Guid CartId,
            Guid? AddressId,
            NewAddressRequest? NewAddress,
            bool IsGift,
            GiftRecipientRequest? GiftRecipient,
            PaymentMethodEnum PaymentMethod) : IRequest<Result<PlaceOrderResponse>>;

       
        public sealed record NewAddressRequest(
            string RecipientName,
            string RecipientPhone,
            string AddressLine,
            string City,
            string Area,
            double? Lat,
            double? Lng);

        public sealed record GiftRecipientRequest(
            string RecipientName,
            string RecipientPhone,
            string RecipientAddress);
}
