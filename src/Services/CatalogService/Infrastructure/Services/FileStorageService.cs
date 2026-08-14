
using Shared.Interfaces;

namespace CatalogService.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var targetDir = Path.Combine(webRoot, "uploads", folder);
            Directory.CreateDirectory(targetDir);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(targetDir, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request is null ? "" : $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/uploads/{folder}/{fileName}";
        }

        public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var relative = new Uri(fileUrl).AbsolutePath.TrimStart('/');
            // strip the leading "uploads/..." segment mapping back to wwwroot
            var fullPath = Path.Combine(_env.ContentRootPath, relative);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            return Task.CompletedTask;
        }
    }
}
