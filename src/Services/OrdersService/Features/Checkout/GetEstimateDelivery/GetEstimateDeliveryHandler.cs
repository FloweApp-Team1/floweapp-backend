using MediatR;
using OrdersService.Features.Checkout.Common;
using OrdersService.Infrastructure.Services;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.Checkout.GetEstimateDelivery
{
    public sealed class GetEstimateDeliveryHandler
       : IRequestHandler<GetEstimateDeliveryQuery, Result<EstimateDeliveryResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAddressServiceClient _addressServiceClient;
        private readonly ICheckoutPricingService _pricingService;

        public GetEstimateDeliveryHandler(
            ICurrentUserService currentUserService,
            IAddressServiceClient addressServiceClient,
            ICheckoutPricingService pricingService)
        {
            _currentUserService = currentUserService;
            _addressServiceClient = addressServiceClient;
            _pricingService = pricingService;
        }

        public async Task<Result<EstimateDeliveryResponse>> Handle(
            GetEstimateDeliveryQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result.Failure<EstimateDeliveryResponse>(Error.New("Order.Unauthorized", "User is not authenticated."));

            var addressResult = await _addressServiceClient.GetAddressForOrderAsync(
                request.AddressId, userId.Value, cancellationToken);

            if (addressResult.IsFailure)
                return Result.Failure<EstimateDeliveryResponse>(addressResult.Error);

            var address = addressResult.Value;

           
            if (!address.IsServiceable || address.StoreId is null)
                return Result.Failure<EstimateDeliveryResponse>(Error.New(
                    "Order.NotServiceable", "This address is outside our current delivery coverage."));

            var pricingResult = await _pricingService.CalculateAsync(request.CartId, userId.Value, address, cancellationToken);
            if (pricingResult.IsFailure)
                return Result.Failure<EstimateDeliveryResponse>(pricingResult.Error);

            var pricing = pricingResult.Value;

            return Result.Success(new EstimateDeliveryResponse(
                address.AddressId,
                IsServiceable: true,
                pricing.DeliveryFee,
                pricing.EstimatedDeliveryAt!.Value));
        }
    }
}
