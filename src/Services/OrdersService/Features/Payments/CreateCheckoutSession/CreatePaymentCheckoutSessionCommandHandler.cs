using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using Shared.Events.OrderEvents;
using Shared.Interfaces;
using Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Http;

using System.Text.Json;
using System.Net.Http.Json;

namespace OrdersService.Features.Payments.CreateCheckoutSession
{
    public class CreatePaymentCheckoutSessionCommandHandler : IRequestHandler<CreatePaymentCheckoutSessionCommand, Result<CreatePaymentCheckoutSessionResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIntegrationEventPublisher _publishEndpoint;
        private readonly HttpClient _paymentServiceClient;
        private readonly ILogger<CreatePaymentCheckoutSessionCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreatePaymentCheckoutSessionCommandHandler(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogServiceClient,
            ICurrentUserService currentUserService,
            IIntegrationEventPublisher publishEndpoint,
            IHttpClientFactory httpClientFactory,
            ILogger<CreatePaymentCheckoutSessionCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _catalogServiceClient = catalogServiceClient;
            _currentUserService = currentUserService;
            _publishEndpoint = publishEndpoint;
            _paymentServiceClient = httpClientFactory.CreateClient("PaymentServiceClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<CreatePaymentCheckoutSessionResponse>> Handle(CreatePaymentCheckoutSessionCommand request, CancellationToken cancellationToken)
        {
            if (request.Items == null || !request.Items.Any())
                return Result<CreatePaymentCheckoutSessionResponse>.Failure(Error.New("Validation", "Order must contain at least one item."));

            if (request.Items.Any(i => i.Quantity <= 0))
                return Result<CreatePaymentCheckoutSessionResponse>.Failure(Error.New("Validation", "Quantity must be greater than zero."));

            var userId = _currentUserService.UserId ?? Guid.Empty;
            var orderRepo = _unitOfWork.Repository<Order>();

            var existingPendingOrder = await orderRepo.FirstOrDefaultAsync(o => 
                o.UserId == userId && 
                o.PaymentStatus == PaymentStatusEnum.Pending &&
                o.PaymentMethod == PaymentMethodEnum.Card);
            if (existingPendingOrder != null)
            {
                return Result<CreatePaymentCheckoutSessionResponse>.Failure(Error.New("Conflict", "You already have a pending order. Please complete or cancel it first."));
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                UserId = userId,
                StoreId = request.StoreId,
                AddressId = request.AddressId,
                IsGift = request.IsGift,
                GiftRecipientName = request.GiftRecipientName,
                GiftRecipientPhone = request.GiftRecipientPhone,
                GiftRecipientAddress = request.GiftRecipientAddress,
                PaymentMethod = PaymentMethodEnum.Card,
                Status = OrderStatusEnum.Placed,
                PaymentStatus = PaymentStatusEnum.Pending
            };

            var catalogTasks = request.Items.Select(async item =>
            {
                var product = await _catalogServiceClient.GetProductDetailsAsync(item.ProductId, cancellationToken);
                return (Item: item, Product: product);
            }).ToList();

            var lookups = await Task.WhenAll(catalogTasks);

            var missing = lookups.FirstOrDefault(l => l.Product == null);
            if (missing.Item != null)
                return Result<CreatePaymentCheckoutSessionResponse>.Failure(Error.New("NotFound", $"Product with ID {missing.Item.ProductId} not found."));

            decimal calculatedSubtotal = 0;

            foreach (var lookup in lookups)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = lookup.Item.ProductId,
                    ProductName = lookup.Product!.Name,
                    ProductImageUrl = lookup.Product.ImageUrl,
                    UnitPrice = lookup.Product.Price, // from CatalogService
                    Quantity = lookup.Item.Quantity,
                    OrderId = order.Id
                };

                calculatedSubtotal += orderItem.UnitPrice * orderItem.Quantity;
                order.Items.Add(orderItem);
            }

            order.Subtotal = calculatedSubtotal;
            order.DeliveryFee = 5.0m; // Example fixed fee "for now"
            order.Total = order.Subtotal + order.DeliveryFee;

            // Convert to cents for PaymentService (Stripe)
            long amountTotalCents = (long)Math.Round(order.Total * 100);

            var checkoutRequest = new
            {
                OrderId = order.Id,
                AmountTotal = amountTotalCents,
                Currency = "usd"
            };

            CreatePaymentCheckoutSessionResponse? checkoutResponse = null;

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/checkout");
                
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader["Bearer ".Length..].Trim();
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                requestMessage.Content = JsonContent.Create(checkoutRequest);
                var response = await _paymentServiceClient.SendAsync(requestMessage, cancellationToken);
                
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<PaymentResultDto<PaymentCheckoutSessionDto>>(responseString, options);

                if (result != null && result.IsSuccess && result.Value != null)
                {
                    checkoutResponse = new CreatePaymentCheckoutSessionResponse(
                        result.Value.CheckoutUrl, 
                        result.Value.StripeSessionId, 
                        result.Value.PaymentAttemptId, 
                        order.Id);
                        
                    order.LastPaymentAttemptId = checkoutResponse.PaymentAttemptId;
                }
                else
                {
                    _logger.LogWarning("PaymentService returned a failure result. Saving order as Pending anyway for retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create checkout session with PaymentService.");
            }

            await orderRepo.AddAsync(order);
            
            // OrderPlacedEvent is no longer published here. Cart is cleared when payment is confirmed.

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (checkoutResponse != null)
            {
                return Result<CreatePaymentCheckoutSessionResponse>.Success(checkoutResponse);
            }

            return Result<CreatePaymentCheckoutSessionResponse>.Success(new CreatePaymentCheckoutSessionResponse(string.Empty, string.Empty, Guid.Empty, order.Id));
        }

        private class PaymentResultDto<T>
        {
            public bool IsSuccess { get; set; }
            public T? Value { get; set; }
        }

        private class PaymentCheckoutSessionDto
        {
            public string CheckoutUrl { get; set; } = string.Empty;
            public string StripeSessionId { get; set; } = string.Empty;
            public Guid PaymentAttemptId { get; set; }
        }
    }
}
