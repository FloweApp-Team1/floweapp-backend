namespace OrdersService.Infrastructure.Services
{
   
        public interface ICartServiceClient
        {
            Task<CartDetailsDto?> GetCartAsync(Guid cartId, Guid userId, CancellationToken cancellationToken);
        }

        public sealed record CartDetailsDto(Guid CartId, IReadOnlyList<CartItemDto> Items);

        public sealed record CartItemDto(Guid ProductId, int Quantity);
}

