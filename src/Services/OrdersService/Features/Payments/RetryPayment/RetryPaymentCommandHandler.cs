using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Services;
using Shared.Interfaces;
using Shared.Results;
using MediatR;
using System.Text.Json;
using System.Net.Http.Json;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace OrdersService.Features.Payments.RetryPayment
{
    public class RetryPaymentCommandHandler : IRequestHandler<RetryPaymentCommand, Result<RetryPaymentResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly HttpClient _paymentServiceClient;
        private readonly ILogger<RetryPaymentCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RetryPaymentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IHttpClientFactory httpClientFactory,
            ILogger<RetryPaymentCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _paymentServiceClient = httpClientFactory.CreateClient("PaymentServiceClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<RetryPaymentResponse>> Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var orderRepo = _unitOfWork.Repository<Order>();

            var order = await orderRepo.GetByIdAsync(request.OrderId);
            if (order == null || order.UserId != userId)
                return Result<RetryPaymentResponse>.Failure(Error.New("NotFound", "Order not found."));

            if (order.PaymentMethod != PaymentMethodEnum.Card)
                return Result<RetryPaymentResponse>.Failure(Error.New("Validation", "Only card orders can be retried."));

            if (order.PaymentStatus == PaymentStatusEnum.Paid)
                return Result<RetryPaymentResponse>.Failure(Error.New("Validation", "Order is already paid."));

            if (order.Status == OrderStatusEnum.Cancelled || order.Status == OrderStatusEnum.Delivered)
                return Result<RetryPaymentResponse>.Failure(Error.New("Validation", "Order cannot be retried in its current state."));

            // Calculate total for Stripe. Amount must be in cents.
            long amountTotalCents = (long)Math.Round(order.Total * 100);

            var checkoutRequest = new
            {
                OrderId = order.Id,
                AmountTotal = amountTotalCents,
                Currency = "usd"
            };

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
                var result = JsonSerializer.Deserialize<PaymentResultDto<RetryPaymentResponse>>(responseString, options);

                if (result != null && result.IsSuccess && result.Value != null)
                {
                    // Update the LastPaymentAttemptId to avoid race conditions with webhooks
                    order.LastPaymentAttemptId = result.Value.PaymentAttemptId;
                    
                    // The EF Core RowVersion will automatically guard against double-clicks
                    orderRepo.Update(order);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return Result<RetryPaymentResponse>.Success(result.Value);
                }
                
                return Result<RetryPaymentResponse>.Failure(Error.New("PaymentServiceError", "Failed to retrieve a valid checkout session."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call PaymentService for RetryPayment on order {OrderId}", order.Id);
                return Result<RetryPaymentResponse>.Failure(Error.New("PaymentServiceError", "Payment service is currently unavailable."));
            }
        }

        private class PaymentResultDto<T>
        {
            public bool IsSuccess { get; set; }
            public T? Value { get; set; }
        }
    }
}
