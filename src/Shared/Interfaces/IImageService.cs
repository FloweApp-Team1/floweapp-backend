namespace Shared.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile image, string folderName, string? UserSubFolder=null,CancellationToken cancellationToken = default);
        Task DeleteImageAsync(string imagePath, CancellationToken cancellationToken = default);
        Task<bool> IsImageExistAsync(string imagePath, CancellationToken cancellationToken = default);
    }
}
