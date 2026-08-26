using MediatR;
using OrdersService.Features.Checkout.Common;
using OrdersService.Infrastructure.Services;
using Shared.Interfaces;
using Shared.Results;

namespace OrdersService.Features.Checkout.GetCheckoutDetails
{
    public sealed class GetCheckoutDetailsHandler
        : IRequestHandler<GetCheckoutDetailsQuery, Result<CheckoutDetailsResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAddressServiceClient _addressServiceClient;
        private readonly ICheckoutPricingService _pricingService;
        private readonly IPaymentMethodProvider _paymentMethodProvider;

        public GetCheckoutDetailsHandler(
            ICurrentUserService currentUserService,
            IAddressServiceClient addressServiceClient,
            ICheckoutPricingService pricingService,
            IPaymentMethodProvider paymentMethodProvider)
        {
            _currentUserService = currentUserService;
            _addressServiceClient = addressServiceClient;
            _pricingService = pricingService;
            _paymentMethodProvider = paymentMethodProvider;
        }

        public async Task<Result<CheckoutDetailsResponse>> Handle(
            GetCheckoutDetailsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId is null)
                return Result.Failure<CheckoutDetailsResponse>(Error.New("Order.Unauthorized", "User is not authenticated."));

            var addressTask = _addressServiceClient.GetDefaultOrLastUsedAddressAsync(userId.Value, cancellationToken);
            var paymentMethodsTask = _paymentMethodProvider.GetAvailableMethodsAsync(cancellationToken);

            await Task.WhenAll(addressTask, paymentMethodsTask);

            var addressResult = await addressTask;
            if (addressResult.IsFailure)
                return Result.Failure<CheckoutDetailsResponse>(addressResult.Error);

            var address = addressResult.Value; 

            var pricingResult = await _pricingService.CalculateAsync(request.CartId, userId.Value, address, cancellationToken);
            if (pricingResult.IsFailure)
                return Result.Failure<CheckoutDetailsResponse>(pricingResult.Error);

            var pricing = pricingResult.Value;
            var paymentMethods = await paymentMethodsTask;

            return Result.Success(new CheckoutDetailsResponse(
                request.CartId,
                address?.AddressId,
                address?.IsServiceable ?? false,
                pricing.Subtotal,
                pricing.DeliveryFee,
                pricing.Total,
                pricing.EstimatedDeliveryAt,
                paymentMethods,
                IsGift: false,
                GiftRecipientName: null,
                GiftRecipientPhone: null));
        }
    }
}
