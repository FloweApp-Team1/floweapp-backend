using OrdersService.Domain.Entities;
using Shared.Interfaces;
using Shared.Security;
using System.Security.Claims;

namespace OrdersService.Infrastructure.Services
{
    public class DriverSnapshotService : IDriverSnapshotService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DriverSnapshotService> _logger;

        public DriverSnapshotService(
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            ILogger<DriverSnapshotService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> EnsureSnapshotAsync(
            Order order, CancellationToken cancellationToken = default)
        {
            if (order.DriverId is not { } driverId)
                return false;

            // Filled once and then left alone: the card should keep showing who actually
            // delivered the order even if that driver later changes their name or photo.
            if (!string.IsNullOrWhiteSpace(order.DriverName))
                return false;

            // The token only describes whoever is holding it, so it may only ever be
            // written onto an order that same person is driving. Without this check a
            // customer opening their own tracking screen would stamp their own name into
            // the driver card.
            if (_currentUser.UserId != driverId)
                return false;

            var claims = _httpContextAccessor.HttpContext?.User;

            if (claims is null)
                return false;

            var fullName = FullName(claims);

            // A driver token with no name claim means IdentityService stopped issuing one.
            // Writing an empty name would permanently poison the snapshot, since the fill
            // only ever happens once - better to leave it for a later attempt.
            if (string.IsNullOrWhiteSpace(fullName))
            {
                _logger.LogWarning(
                    "Driver {DriverId} has no name claim on their token; order {OrderId} keeps an empty driver card.",
                    driverId, order.Id);

                return false;
            }

            order.DriverName = fullName;

            // Absent until IdentityService adds them to the token. Read anyway so the card
            // completes itself the day they appear, with no change needed here.
            order.DriverPhone = Claim(claims, AppClaimTypes.PhoneNumber);
            order.DriverImageUrl = Claim(claims, AppClaimTypes.ImageUrl);

            // DriverAssignedAt is deliberately not invented here. It belongs to whoever
            // assigns the driver; stamping it at snapshot time would tell the customer
            // a driver was assigned at a moment that is simply not true.

            await PersistAsync(order, cancellationToken);

            return true;
        }

        private static string FullName(ClaimsPrincipal user)
        {
            var first = Claim(user, AppClaimTypes.FirstName);
            var last = Claim(user, AppClaimTypes.LastName);

            var combined = $"{first} {last}".Trim();

            // Falls back to the standard name claim so a token shaped differently to
            // today's still produces something the customer can read.
            return string.IsNullOrWhiteSpace(combined)
                ? Claim(user, ClaimTypes.Name) ?? string.Empty
                : combined;
        }

        private static string? Claim(ClaimsPrincipal user, string type)
        {
            var value = user.FindFirstValue(type);

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        // The order handed in may have come from a no-tracking read, so the columns are
        // written through a tracked load rather than by attaching an entity this context
        // does not own.
        private async Task PersistAsync(Order order, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Order>();
            var tracked = await repository.GetByIdAsync(order.Id, cancellationToken);

            if (tracked is null)
                return;

            tracked.DriverName = order.DriverName;
            tracked.DriverPhone = order.DriverPhone;
            tracked.DriverImageUrl = order.DriverImageUrl;
            tracked.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
