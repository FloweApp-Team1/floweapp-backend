using OrdersService.Features.Checkout.PlaceOrder;
using OrdersService.Infrastructure.Clients;
using Shared.Results;

namespace OrdersService.Infrastructure.Services
{
   
        public interface IAddressServiceClient
        {
            Task<Result<OrderAddressDetails>> GetAddressForOrderAsync(
                Guid addressId, Guid userId, CancellationToken cancellationToken);

            Task<Result<OrderAddressDetails>> CreateAddressForOrderAsync(
                Guid userId, NewAddressRequestPayload newAddress, CancellationToken cancellationToken);
        }

        public sealed record OrderAddressDetails(
            Guid AddressId,
            string RecipientName,
            string RecipientPhone,
            string AddressLine,
            string City,
            string Area,
            double? Lat,
            double? Lng,
            Guid? StoreId,
            bool IsServiceable);
    }


