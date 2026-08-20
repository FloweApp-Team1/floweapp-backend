using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
            where T : class;
    }
}
