namespace IdentityService.Common.Interfaces
{
    public interface IFcmService
    {
        Task SendSilentDataMessageAsync(
            IReadOnlyList<string> deviceTokens,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default);
    }
}
