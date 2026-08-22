using AddressCartService.Domain.Entities;
using AddressCartService.Features.Cart.AddCartItem;
using AddressCartService.Features.Cart.GetCart;
using AddressCartService.Features.Cart.RemoveCartItem;
using AddressCartService.Features.Cart.UpdateCartItem;
using AddressCartService.Infrastructure.Consumers;
using AddressCartService.Infrastructure.Persistence;
using AddressCartService.Infrastructure.Repositories;
using AddressCartService.Infrastructure.Services.Catalog;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Events.IntegrationEvents;
using Shared.Events.OrderEvents;
using Shared.Interfaces;

namespace AddressCartService.Tests
{
    /// <summary>
    /// Test-specific DbContext that disables the RowVersion optimistic-concurrency token.
    /// The EF Core InMemory provider auto-increments RowVersion on every SaveChanges, which
    /// triggers a false DbUpdateConcurrencyException between Remove and Re-Add calls in tests
    /// when the ChangeTracker is cleared mid-test (the reloaded entity gets a stale RowVersion
    /// snapshot that doesn't match EF's internal concurrency state).
    /// </summary>
    internal class TestAddressCartDbContext : AddressCartDbContext
    {
        // Use DbContextOptions<TestAddressCartDbContext> so EF Core's model cache is keyed on this
        // derived type, ensuring OnModelCreating below is actually called (not the cached base model).
        public TestAddressCartDbContext(DbContextOptions<TestAddressCartDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Ignore RowVersion entirely in tests.
            // IsRowVersion() configures both IsConcurrencyToken and ValueGeneratedOnAddOrUpdate;
            // IsConcurrencyToken(false) alone does not fully override it.
            // Ignoring the property removes it from the model so EF never checks it,
            // preventing false DbUpdateConcurrencyExceptions in single-threaded test scenarios.
            builder.Entity<Cart>().Ignore(x => x.RowVersion);
        }
    }

    public class CartFeatureTests
    {
        private readonly AddressCartDbContext _dbContext;
        private readonly UnitOfWork _unitOfWork;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<ICatalogClient> _catalogClientMock;
        private readonly Mock<ISender> _senderMock;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _otherUserId = Guid.NewGuid();

        public CartFeatureTests()
        {
            var options = new DbContextOptionsBuilder<TestAddressCartDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TestAddressCartDbContext(options);
            _unitOfWork = new UnitOfWork(_dbContext);
            _currentUserMock = new Mock<ICurrentUserService>();
            _catalogClientMock = new Mock<ICatalogClient>();
            _senderMock = new Mock<ISender>();

            _currentUserMock.Setup(x => x.UserId).Returns(_userId);
        }

        #region AddCartItem Tests

        [Fact]
        public async Task AddCartItem_EmptyCart_AddsItemSuccessfully()
        {
            var productId = Guid.NewGuid();
            var product = new CatalogProductDto(productId, "Red Roses", 100m, 100m, null, null, true, 10, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new AddCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());
            var command = new AddCartItemCommand(productId, 2);

            var result = await handler.Handle(command, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Quantity);
            Assert.Equal(100m, result.Value.PriceAtAdd);

            var cart = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == _userId);
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

            var handler = new AddCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            await handler.Handle(new AddCartItemCommand(productId, 1), default);
            var result = await handler.Handle(new AddCartItemCommand(productId, 2), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.Quantity);

            var cart = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == _userId);
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

            var handler = new AddCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

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

            var handler = new AddCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            var result = await handler.Handle(new AddCartItemCommand(productId, 1), default);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task AddCartItem_NonexistentProduct_Fails()
        {
            var productId = Guid.NewGuid();
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync((CatalogProductDto?)null);

            var handler = new AddCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

            var result = await handler.Handle(new AddCartItemCommand(productId, 1), default);

            Assert.True(result.IsFailure);
            Assert.Equal("Product.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task AddCartItem_Unauthenticated_Fails()
        {
            _currentUserMock.Setup(x => x.UserId).Returns((Guid?)null);
            var handler = new AddCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, Mock.Of<ILogger<AddCartItemHandler>>());

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
            var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = productId, Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var product = new CatalogProductDto(productId, "Tulips", 50m, 50m, null, null, true, 10, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            _senderMock.Setup(x => x.Send(It.IsAny<GetCartQuery>(), default))
                .ReturnsAsync(Shared.Results.Result.Success(new GetCartResponse(cart.Id, new List<CartItemResponse>(), 5, 1, 250m, null, 250m, false)));

            var handler = new UpdateCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, _senderMock.Object, Mock.Of<ILogger<UpdateCartItemHandler>>());
            var result = await handler.Handle(new UpdateCartItemCommand(item.Id, 5), default);

            Assert.True(result.IsSuccess);
            var updatedItem = await _dbContext.CartItems.FindAsync(item.Id);
            Assert.Equal(5, updatedItem!.Quantity);
        }

        [Fact]
        public async Task UpdateCartItem_ExceedsStock_Fails()
        {
            var productId = Guid.NewGuid();
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = productId, Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var product = new CatalogProductDto(productId, "Tulips", 50m, 50m, null, null, true, 3, "http://image.jpg", false);
            _catalogClientMock.Setup(x => x.GetProductByIdAsync(productId, null, default)).ReturnsAsync(product);

            var handler = new UpdateCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, _senderMock.Object, Mock.Of<ILogger<UpdateCartItemHandler>>());
            var result = await handler.Handle(new UpdateCartItemCommand(item.Id, 5), default);

            Assert.True(result.IsFailure);
            Assert.Contains("Conflict", result.Error.Code);
        }

        [Fact]
        public async Task UpdateCartItem_OtherUserItem_FailsNotFound()
        {
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _otherUserId };
            var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var handler = new UpdateCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _catalogClientMock.Object, _senderMock.Object, Mock.Of<ILogger<UpdateCartItemHandler>>());
            var result = await handler.Handle(new UpdateCartItemCommand(item.Id, 5), default);

            Assert.True(result.IsFailure);
            Assert.Equal("CartItem.NotFound", result.Error.Code);
        }

        #endregion

        #region RemoveCartItem & Hard Delete Tests

        [Fact]
        public async Task RemoveCartItem_OwnedItem_HardDeletesItem()
        {
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            _senderMock.Setup(x => x.Send(It.IsAny<GetCartQuery>(), default))
                .ReturnsAsync(Shared.Results.Result.Success(new GetCartResponse(cart.Id, new List<CartItemResponse>(), 0, 0, 0m, null, 0m, false)));

            var handler = new RemoveCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _senderMock.Object, Mock.Of<ILogger<RemoveCartItemHandler>>());
            var result = await handler.Handle(new RemoveCartItemCommand(item.Id), default);

            Assert.True(result.IsSuccess);
            var deletedItem = await _dbContext.CartItems.FindAsync(item.Id);
            Assert.Null(deletedItem); // Hard-deleted cleanly!
        }

        [Fact]
        public async Task RemoveAndReAdd_SameProduct_NoUniqueConstraintBug()
        {
            // Seed Cart + CartItem directly to avoid AddCartItemHandler's IsRowVersion auto-generation.
            // Going through AddCartItemHandler twice causes EF InMemory to auto-increment RowVersion
            // on the first save, making the second handler call see a stale concurrency snapshot —
            // a false positive that masks the real bug this test is verifying.
            var productId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            var cart = new Cart
            {
                Id = cartId, UserId = _userId,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, LastChangedBy = _userId
            };
            var item = new CartItem
            {
                Id = itemId, CartId = cartId, ProductId = productId,
                Quantity = 1, PriceAtAdd = 80m,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, LastChangedBy = _userId
            };
            cart.Items.Add(item);
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            _senderMock.Setup(x => x.Send(It.IsAny<GetCartQuery>(), default))
                .ReturnsAsync(Shared.Results.Result.Success(new GetCartResponse(cartId, new List<CartItemResponse>(), 0, 0, 0m, null, 0m, false)));

            var removeHandler = new RemoveCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _senderMock.Object, Mock.Of<ILogger<RemoveCartItemHandler>>());

            // Remove via handler
            var remRes = await removeHandler.Handle(new RemoveCartItemCommand(itemId), default);
            Assert.True(remRes.IsSuccess);

            // Verify item is HARD-deleted: FindAsync with IgnoreQueryFilters returns null —
            // a soft-deleted row would still be visible here.
            var ghost = await _dbContext.CartItems.IgnoreQueryFilters().FindAsync(itemId);
            Assert.Null(ghost);

            // Verify re-insertion with same (CartId, ProductId) succeeds — no unique-constraint ghost row.
            // In production (SQL Server) this would fail if the item were only soft-deleted.
            var readdedItem = new CartItem
            {
                Id = Guid.NewGuid(), CartId = cartId, ProductId = productId,
                Quantity = 2, PriceAtAdd = 80m,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, LastChangedBy = _userId
            };
            _dbContext.CartItems.Add(readdedItem);
            var ex = await Record.ExceptionAsync(() => _dbContext.SaveChangesAsync());
            Assert.Null(ex);
        }

        [Fact]
        public async Task RemoveCartItem_OtherUserItem_FailsNotFound()
        {
            var cart = new Cart { Id = Guid.NewGuid(), UserId = _otherUserId };
            var item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 50m };
            cart.Items.Add(item);
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var handler = new RemoveCartItemHandler(_dbContext, _unitOfWork, _currentUserMock.Object, _senderMock.Object, Mock.Of<ILogger<RemoveCartItemHandler>>());
            var result = await handler.Handle(new RemoveCartItemCommand(item.Id), default);

            Assert.True(result.IsFailure);
            Assert.Equal("CartItem.NotFound", result.Error.Code);
        }

        #endregion

        #region GetCart & Calculation Tests

        [Fact]
        public async Task GetCart_EmptyCart_ReturnsValidEmptyCartResponse()
        {
            var handler = new GetCartQueryHandler(_dbContext, _currentUserMock.Object, _catalogClientMock.Object);
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
            cart.Items.Add(new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = p1, Quantity = 2, PriceAtAdd = 100m });
            cart.Items.Add(new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = p2, Quantity = 5, PriceAtAdd = 50m });
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var batchDict = new Dictionary<Guid, CatalogProductDto>
            {
                [p1] = new CatalogProductDto(p1, "Roses", 100m, 120m, null, null, true, 10, "http://r.jpg", false), // Price changed 100 -> 120
                [p2] = new CatalogProductDto(p2, "Chocolates", 50m, 50m, null, null, true, 3, "http://c.jpg", false) // Stock changed (requested 5 > 3)
            };
            _catalogClientMock.Setup(x => x.GetProductsBatchAsync(It.IsAny<IEnumerable<Guid>>(), null, default))
                .ReturnsAsync(batchDict);

            var handler = new GetCartQueryHandler(_dbContext, _currentUserMock.Object, _catalogClientMock.Object);
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
        public async Task OrderPlacedEventConsumer_ClearsOnlyUserCart()
        {
            var userCart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            userCart.Items.Add(new CartItem { Id = Guid.NewGuid(), CartId = userCart.Id, ProductId = Guid.NewGuid(), Quantity = 2, PriceAtAdd = 10m });
            _dbContext.Carts.Add(userCart);

            var otherCart = new Cart { Id = Guid.NewGuid(), UserId = _otherUserId };
            otherCart.Items.Add(new CartItem { Id = Guid.NewGuid(), CartId = otherCart.Id, ProductId = Guid.NewGuid(), Quantity = 1, PriceAtAdd = 20m });
            _dbContext.Carts.Add(otherCart);

            await _dbContext.SaveChangesAsync();

            var consumer = new OrderPlacedEventConsumer(_dbContext, Mock.Of<ILogger<OrderPlacedEventConsumer>>());
            var consumeContextMock = new Mock<ConsumeContext<OrderPlacedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(new OrderPlacedEvent { OrderId = Guid.NewGuid(), UserId = _userId });

            await consumer.Consume(consumeContextMock.Object);

            var clearedCart = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == _userId);
            Assert.NotNull(clearedCart);
            Assert.Empty(clearedCart.Items); // Target user cart cleared

            var unchangedCart = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == _otherUserId);
            Assert.NotNull(unchangedCart);
            Assert.Single(unchangedCart.Items); // Other user cart intact
        }

        [Fact]
        public async Task OrderPlacedEventConsumer_AlreadyEmptyCart_IsIdempotentAndSafe()
        {
            var userCart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
            _dbContext.Carts.Add(userCart);
            await _dbContext.SaveChangesAsync();

            var consumer = new OrderPlacedEventConsumer(_dbContext, Mock.Of<ILogger<OrderPlacedEventConsumer>>());
            var consumeContextMock = new Mock<ConsumeContext<OrderPlacedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(new OrderPlacedEvent { OrderId = Guid.NewGuid(), UserId = _userId });

            await consumer.Consume(consumeContextMock.Object);

            var cart = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == _userId);
            Assert.NotNull(cart);
            Assert.Empty(cart.Items);
        }

        #endregion
    }
}
