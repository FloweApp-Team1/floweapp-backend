using AddressCartService.Domain.Entities;
using AddressCartService.Features.Cart;
using AddressCartService.Features.Cart.AddCartItem;
using AddressCartService.Features.Cart.GetCart;
using AddressCartService.Features.Cart.RemoveCartItem;
using AddressCartService.Features.Cart.UpdateCartItem;
using AddressCartService.Infrastructure.Consumers;
using AddressCartService.Infrastructure.Services.Catalog;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using Shared.Events.OrderEvents;
using Shared.Interfaces;

namespace AddressCartService.Tests
{
    public class CartFeatureTests
    {
        // In-memory stand-in for Redis: handlers only ever Get/Set/Remove by cache key,
        // so a plain dictionary reproduces real cross-call persistence without a live Redis instance.
        private readonly Dictionary<string, object> _cacheStore = new();
        private readonly Mock<IRedisCacheService> _redisCacheMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<ICatalogClient> _catalogClientMock;
        private readonly Mock<ISender> _senderMock;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _otherUserId = Guid.NewGuid();

        public CartFeatureTests()
        {
            _redisCacheMock = new Mock<IRedisCacheService>();
            _redisCacheMock
                .Setup(x => x.GetAsync<Cart>(It.IsAny<string>()))
                .Returns<string>(key => Task.FromResult(
                    _cacheStore.TryGetValue(key, out var value) ? (Cart?)value : null));
            _redisCacheMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Cart>(), It.IsAny<TimeSpan>()))
                .Returns<string, Cart, TimeSpan>((key, value, _) =>
                {
                    _cacheStore[key] = value;
                    return Task.CompletedTask;
                });
            _redisCacheMock
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    _cacheStore.Remove(key);
                    return Task.CompletedTask;
                });

            _currentUserMock = new Mock<ICurrentUserService>();
            _catalogClientMock = new Mock<ICatalogClient>();
            _senderMock = new Mock<ISender>();

            _currentUserMock.Setup(x => x.UserId).Returns(_userId);
        }

        private Cart? GetCachedCart(Guid userId) =>
            _cacheStore.TryGetValue(CartCacheKeys.Cart(userId), out var value) ? (Cart)value : null;

        #region AddCartItem Tests

        [Fact]
        public async Task AddCartItem_EmptyCart_AddsItemSuccessfully()
        {
            var productId = Guid.NewGuid();
            var product = new CatalogProductDto(productId, "Red Roses", 100m, 100m, null, null, true, 10, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());
            var command = new AddCartItemCommand(productId, 2);

            var result = await handler.Handle(command, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Quantity);
            Assert.Equal(100m, result.Value.PriceAtAdd);

            var cart = GetCachedCart(_userId);
            Assert.NotNull(cart);
            Assert.Single(cart.Items);
            Assert.Equal(2, cart.Items.First().Quantity);
        }

        [Fact]
        public async Task AddCartItem_SameProductTwice_IncrementsQuantityNoDuplicateLine()
        {
            var productId = Guid.NewGuid();
            var product = new CatalogProductDto(productId, "Red Roses", 100m, 100m, null, null, true, 10, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            await handler.Handle(new AddCartItemCommand(productId, 1), default);
            var result = await handler.Handle(new AddCartItemCommand(productId, 2), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.Quantity);

            var cart = GetCachedCart(_userId);
            Assert.NotNull(cart);
            Assert.Single(cart.Items);
            Assert.Equal(3, cart.Items.First().Quantity);
        }

        [Fact]
        public async Task AddCartItem_ExceedsStock_FailsWithConflictError()
        {
            var productId = Guid.NewGuid();
            var product = new CatalogProductDto(productId, "Red Roses", 100m, 100m, null, null, true, 3, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            var result = await handler.Handle(new AddCartItemCommand(productId, 5), default);

            Assert.True(result.IsFailure);
            Assert.Contains("Conflict", result.Error.Code);
        }

        [Fact]
        public async Task AddCartItem_OutOfStockProduct_Fails()
        {
            var productId = Guid.NewGuid();
            var product = new CatalogProductDto(productId, "Red Roses", 100m, 100m, null, null, true, 0, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            var result = await handler.Handle(new AddCartItemCommand(productId, 1), default);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task AddCartItem_NonexistentProduct_Fails()
        {
            var productId = Guid.NewGuid();
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync((CatalogProductDto?)null);

            var handler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            var result = await handler.Handle(new AddCartItemCommand(productId, 1), default);

            Assert.True(result.IsFailure);
            Assert.Equal("Product.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task AddCartItem_Unauthenticated_Fails()
        {
            _currentUserMock.Setup(x => x.UserId).Returns((Guid?)null);
            var handler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            var result = await handler.Handle(new AddCartItemCommand(Guid.NewGuid(), 1), default);

            Assert.True(result.IsFailure);
            Assert.Equal("Cart.Unauthorized", result.Error.Code);
        }

        #endregion

        #region UpdateCartItem Tests

        [Fact]
        public async Task UpdateCartItem_ValidQuantity_UpdatesSuccessfully()
        {
            var productId = Guid.NewGuid();
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            var item = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _cacheStore[CartCacheKeys.Cart(_userId)] = cart;

            var product = new CatalogProductDto(productId, "Tulips", 50m, 50m, null, null, true, 10, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            _senderMock.Setup(x => x.Send(It.IsAny<GetCartQuery>(), default))
                .ReturnsAsync(Shared.Results.Result.Success(new GetCartResponse(cart.Id, new List<CartItemResponse>(), 5, 1, 250m, null, 250m, false)));

            var handler = new UpdateCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, _senderMock.Object, Mock.Of<ILogger<UpdateCartItemHandler>>());
            var result = await handler.Handle(new UpdateCartItemCommand(item.Id, 5), default);

            Assert.True(result.IsSuccess);
            var updatedItem = GetCachedCart(_userId)!.Items.First(i => i.Id == item.Id);
            Assert.Equal(5, updatedItem.Quantity);
        }

        [Fact]
        public async Task UpdateCartItem_ExceedsStock_Fails()
        {
            var productId = Guid.NewGuid();
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            var item = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _cacheStore[CartCacheKeys.Cart(_userId)] = cart;

            var product = new CatalogProductDto(productId, "Tulips", 50m, 50m, null, null, true, 3, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new UpdateCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, _senderMock.Object, Mock.Of<ILogger<UpdateCartItemHandler>>());
            var result = await handler.Handle(new UpdateCartItemCommand(item.Id, 5), default);

            Assert.True(result.IsFailure);
            Assert.Contains("Conflict", result.Error.Code);
        }

        [Fact]
        public async Task UpdateCartItem_OtherUserItem_FailsNotFound()
        {
            // The current user (_userId) has no cart of their own cached, so looking up an
            // item that only exists in _otherUserId's cart correctly misses regardless of ItemId.
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _otherUserId };
            var item = new CartItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _cacheStore[CartCacheKeys.Cart(_otherUserId)] = cart;

            var handler = new UpdateCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, _senderMock.Object, Mock.Of<ILogger<UpdateCartItemHandler>>());
            var result = await handler.Handle(new UpdateCartItemCommand(item.Id, 5), default);

            Assert.True(result.IsFailure);
            Assert.Equal("CartItem.NotFound", result.Error.Code);
        }

        #endregion

        #region RemoveCartItem Tests

        [Fact]
        public async Task RemoveCartItem_OwnedItem_RemovesItemFromCart()
        {
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            var item = new CartItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _cacheStore[CartCacheKeys.Cart(_userId)] = cart;

            _senderMock.Setup(x => x.Send(It.IsAny<GetCartQuery>(), default))
                .ReturnsAsync(Shared.Results.Result.Success(new GetCartResponse(cart.Id, new List<CartItemResponse>(), 0, 0, 0m, null, 0m, false)));

            var handler = new RemoveCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _senderMock.Object, Mock.Of<ILogger<RemoveCartItemHandler>>());
            var result = await handler.Handle(new RemoveCartItemCommand(item.Id), default);

            Assert.True(result.IsSuccess);
            Assert.DoesNotContain(GetCachedCart(_userId)!.Items, i => i.Id == item.Id);
        }

        [Fact]
        public async Task RemoveAndReAdd_SameProduct_Succeeds()
        {
            var productId = Guid.NewGuid();
            var product = new CatalogProductDto(productId, "Red Roses", 100m, 100m, null, null, true, 10, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var addHandler = new AddCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());
            var addResult = await addHandler.Handle(new AddCartItemCommand(productId, 1), default);
            Assert.True(addResult.IsSuccess);
            var itemId = addResult.Value.ItemId;
            var cartId = addResult.Value.CartId;

            _senderMock.Setup(x => x.Send(It.IsAny<GetCartQuery>(), default))
                .ReturnsAsync(Shared.Results.Result.Success(new GetCartResponse(cartId, new List<CartItemResponse>(), 0, 0, 0m, null, 0m, false)));

            var removeHandler = new RemoveCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _senderMock.Object, Mock.Of<ILogger<RemoveCartItemHandler>>());
            var removeResult = await removeHandler.Handle(new RemoveCartItemCommand(itemId), default);
            Assert.True(removeResult.IsSuccess);
            Assert.Empty(GetCachedCart(_userId)!.Items);

            // Re-adding after removal should start a fresh line, not resurrect the removed item's quantity.
            var readdResult = await addHandler.Handle(new AddCartItemCommand(productId, 2), default);
            Assert.True(readdResult.IsSuccess);
            Assert.Equal(2, readdResult.Value.Quantity);
            Assert.Single(GetCachedCart(_userId)!.Items);
        }

        [Fact]
        public async Task RemoveCartItem_OtherUserItem_FailsNotFound()
        {
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _otherUserId };
            var item = new CartItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _cacheStore[CartCacheKeys.Cart(_otherUserId)] = cart;

            var handler = new RemoveCartItemHandler(_redisCacheMock.Object, _currentUserMock.Object, _senderMock.Object, Mock.Of<ILogger<RemoveCartItemHandler>>());
            var result = await handler.Handle(new RemoveCartItemCommand(item.Id), default);

            Assert.True(result.IsFailure);
            Assert.Equal("CartItem.NotFound", result.Error.Code);
        }

        #endregion

        #region GetCart & Calculation Tests

        [Fact]
        public async Task GetCart_EmptyCart_ReturnsValidEmptyCartResponse()
        {
            var handler = new GetCartQueryHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object);
            var result = await handler.Handle(new GetCartQuery(), default);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0, result.Value.TotalQuantity);
            Assert.Equal(0, result.Value.LineCount);
            Assert.Equal(0m, result.Value.Subtotal);
            Assert.Null(result.Value.DeliveryFee);
            Assert.Equal(0m, result.Value.Total);
            Assert.False(result.Value.HasChanges);
        }

        [Fact]
        public async Task GetCart_WithItems_CalculatesTotalsAndDetectsPriceStockChanges()
        {
            var p1 = Guid.NewGuid();
            var p2 = Guid.NewGuid();

            var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            cart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = p1, Quantity = 2, PriceAtAdd = 100m });
            cart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = p2, Quantity = 5, PriceAtAdd = 50m });
            _cacheStore[CartCacheKeys.Cart(_userId)] = cart;

            var batchDict = new Dictionary<Guid, CatalogProductDto>
            {
                [p1] = new CatalogProductDto(p1, "Roses", 100m, 120m, null, null, true, 10, "http://r.jpg", false), // Price changed 100 -> 120
                [p2] = new CatalogProductDto(p2, "Chocolates", 50m, 50m, null, null, true, 3, "http://c.jpg", false) // Stock changed (requested 5 > 3)
            };
            _catalogClientMock.Setup(x => x.GetProductsBatchAsync(It.IsAny<IEnumerable<Guid>>(), null, default))
                .ReturnsAsync(batchDict);

            var handler = new GetCartQueryHandler(_redisCacheMock.Object, _currentUserMock.Object, _catalogClientMock.Object);
            var result = await handler.Handle(new GetCartQuery(), default);

            Assert.True(result.IsSuccess);
            var res = result.Value;
            Assert.Equal(2, res.LineCount);
            Assert.Equal(7, res.TotalQuantity);
            Assert.Equal(2 * 120m + 5 * 50m, res.Subtotal); // 240 + 250 = 490
            Assert.True(res.HasChanges);

            var item1 = res.Items.First(i => i.ProductId == p1);
            Assert.True(item1.PriceChanged);

            var item2 = res.Items.First(i => i.ProductId == p2);
            Assert.True(item2.StockChanged);
        }

        #endregion

        #region RabbitMQ Consumer Tests

        [Fact]
        public async Task ClearCartOnOrderConfirmedConsumer_ClearsOnlyUserCart()
        {
            var userCart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            userCart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 10m });
            _cacheStore[CartCacheKeys.Cart(_userId)] = userCart;

            var otherCart = new Cart { Id = Guid.NewGuid(), UserId = _otherUserId };
            otherCart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, PriceAtAdd = 20m });
            _cacheStore[CartCacheKeys.Cart(_otherUserId)] = otherCart;

            var consumer = new ClearCartOnOrderConfirmedConsumer(_redisCacheMock.Object, Mock.Of<ILogger<ClearCartOnOrderConfirmedConsumer>>());
            var consumeContextMock = new Mock<ConsumeContext<OrderConfirmedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(new OrderConfirmedEvent
            {
                OrderId = Guid.NewGuid(),
                UserId = _userId,
                PaymentMethod = "COD",
                OrderNumber = "ORD-1",
                Total = 20m
            });

            await consumer.Consume(consumeContextMock.Object);

            Assert.Null(GetCachedCart(_userId)); // Target user cart cleared
            var unchangedCart = GetCachedCart(_otherUserId);
            Assert.NotNull(unchangedCart);
            Assert.Single(unchangedCart.Items); // Other user cart intact
        }

        [Fact]
        public async Task ClearCartOnOrderConfirmedConsumer_NoExistingCart_IsIdempotentAndSafe()
        {
            var consumer = new ClearCartOnOrderConfirmedConsumer(_redisCacheMock.Object, Mock.Of<ILogger<ClearCartOnOrderConfirmedConsumer>>());
            var consumeContextMock = new Mock<ConsumeContext<OrderConfirmedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(new OrderConfirmedEvent
            {
                OrderId = Guid.NewGuid(),
                UserId = _userId,
                PaymentMethod = "COD",
                OrderNumber = "ORD-2",
                Total = 0m
            });

            var ex = await Record.ExceptionAsync(() => consumer.Consume(consumeContextMock.Object));

            Assert.Null(ex);
            Assert.Null(GetCachedCart(_userId));
        }

        #endregion
    }
}
