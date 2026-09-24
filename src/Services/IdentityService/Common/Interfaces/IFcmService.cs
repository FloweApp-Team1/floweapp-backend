namespace IdentityService.Common.Interfaces
{
    public interface IFcmService
    {
        Task SendSilentDataMessageAsync(
            IReadOnlyList<string> deviceTokens,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default);

        Task SendNotificationAsync(
            IReadOnlyList<string> deviceTokens,
            string title,
            string body,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default);
    }
}
