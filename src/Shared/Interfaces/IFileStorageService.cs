using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);

        Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    }
}
