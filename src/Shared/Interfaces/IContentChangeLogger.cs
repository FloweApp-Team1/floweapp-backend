using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IContentChangeLogger
    {
        Task LogAsync(
            string entityType,
            Guid entityId,
            string action,
            Guid changedBy,
            string? summary = null,
            CancellationToken cancellationToken = default);
    }
}
