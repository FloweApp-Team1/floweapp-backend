using FirebaseAdmin.Messaging;
using IdentityService.Common.Interfaces;
using Shared.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Notifications
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FcmService(ILogger<FcmService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task SendSilentDataMessageAsync(
            IReadOnlyList<string> deviceTokens,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default)
        {
            if (deviceTokens.Count == 0) return;

            var message = new MulticastMessage
            {
                Tokens = deviceTokens,
                Data = data,
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

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);

            if (response.FailureCount > 0)
            {
                var failedTokens = new List<string>();
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        var exception = response.Responses[i].Exception;
                        // Prune invalid tokens
                        if (exception.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                            exception.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                        {
                            failedTokens.Add(deviceTokens[i]);
                        }
                    }
                }

                if (failedTokens.Count > 0)
                {
                    _logger.LogWarning("Removing {Count} invalid FCM tokens.", failedTokens.Count);
                    var deviceTokenRepo = _unitOfWork.Repository<UserDeviceToken>();
                    
                    var tokensToRemove = await deviceTokenRepo.Query()
                        .Where(x => failedTokens.Contains(x.FcmToken))
                        .ToListAsync(cancellationToken);
                        
                    foreach(var token in tokensToRemove)
                    {
                        deviceTokenRepo.Remove(token);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
