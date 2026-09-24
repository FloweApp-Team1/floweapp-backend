using FirebaseAdmin.Messaging;
using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FirebaseAdmin;

namespace IdentityService.Infrastructure.Notifications
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseMessaging _messaging;

        public FcmService(ILogger<FcmService> logger, IUnitOfWork unitOfWork, FirebaseApp firebaseApp)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _messaging = FirebaseMessaging.GetMessaging(firebaseApp);
        }

        public async Task SendSilentDataMessageAsync(
            IReadOnlyList<string> deviceTokens,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default)
            => await SendAsync(deviceTokens, data, notification: null, cancellationToken);

        public async Task SendNotificationAsync(
            IReadOnlyList<string> deviceTokens,
            string title,
            string body,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default)
            => await SendAsync(
                deviceTokens,
                data,
                new Notification { Title = title, Body = body },
                cancellationToken);

        private async Task SendAsync(
            IReadOnlyList<string> deviceTokens,
            IReadOnlyDictionary<string, string> data,
            Notification? notification,
            CancellationToken cancellationToken)
        {
            var uniqueTokens = deviceTokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (uniqueTokens.Length == 0) return;

            var failedTokens = new HashSet<string>(StringComparer.Ordinal);
            foreach (var tokenBatch in uniqueTokens.Chunk(500))
            {
                var message = new MulticastMessage
                {
                    Tokens = tokenBatch,
                    Data = data,
                    Notification = notification,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High // Required for silent pushes on some devices
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            ContentAvailable = true // Required for iOS background updates
                        }
                    }
                };

                var response = await _messaging.SendEachForMulticastAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Firebase accepted {SuccessCount} of {TokenCount} message(s); {FailureCount} failed.",
                    response.SuccessCount,
                    tokenBatch.Length,
                    response.FailureCount);

                if (response.FailureCount > 0)
                {
                    for (var i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            var exception = response.Responses[i].Exception;
                            _logger.LogWarning(
                                exception,
                                "Firebase rejected a message with code {MessagingErrorCode}.",
                                exception.MessagingErrorCode);

                            // Prune invalid tokens
                            if (exception.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                                exception.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                            {
                                failedTokens.Add(tokenBatch[i]);
                            }
                        }
                    }
                }
            }

            if (failedTokens.Count == 0) return;

            _logger.LogWarning("Removing {Count} invalid FCM tokens.", failedTokens.Count);
            var deviceTokenRepo = _unitOfWork.Repository<UserDeviceToken>();
            var tokensToRemove = await deviceTokenRepo.Query()
                .Where(x => failedTokens.Contains(x.FcmToken))
                .ToListAsync(cancellationToken);

            foreach (var token in tokensToRemove)
                deviceTokenRepo.Remove(token);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
