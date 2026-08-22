using Shared.Interfaces;

namespace IdentityService.Infrastructure.Services
{
    public class LocalImageService : IImageService
    {

        public async Task<string> UploadImageAsync(IFormFile image, string folderName, string? UserSubFolder=null,CancellationToken cancellationToken = default)
        {
            var folderpath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", folderName);
            if (!string.IsNullOrEmpty(UserSubFolder))
            {
                folderpath = Path.Combine(folderpath, UserSubFolder);
            }
            Directory.CreateDirectory(folderpath);

            var extention = Path.GetExtension(image.FileName);
            var FileName = $"{Guid.NewGuid()}{extention}";
            var fullPath = Path.Combine(folderpath, FileName);

            //using var stream=File.Create(fullPath);
            using var stream=new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None,bufferSize: 4096, useAsync: true);
            await image.CopyToAsync(stream,cancellationToken);
            return Path.Combine("Storage", folderName,UserSubFolder ?? string.Empty, FileName)
                .Replace("\\", "/");

        }


        public Task DeleteImageAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            var fullPath=Path.Combine(Directory.GetCurrentDirectory(), imagePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Task.CompletedTask;
        }

        public Task<bool> IsImageExistAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath);
            return Task.FromResult(File.Exists(fullPath));
        }

      
    }
}
