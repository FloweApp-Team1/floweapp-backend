using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.IntegrationEvents
{
    public record ProductCreatedEvent(Guid ProductId, Guid CategoryId, Guid ChangedBy, DateTime ChangedAt);
    public record ProductUpdatedEvent(Guid ProductId, Guid ChangedBy, DateTime ChangedAt);
    public record ProductArchivedEvent(Guid ProductId, Guid ChangedBy, DateTime ChangedAt);

    public record CategoryCreatedEvent(Guid CategoryId, Guid ChangedBy, DateTime ChangedAt);
    public record CategoryUpdatedEvent(Guid CategoryId, Guid ChangedBy, DateTime ChangedAt);
    public record CategoryArchivedEvent(Guid CategoryId, Guid ChangedBy, DateTime ChangedAt);

    public record OccasionCreatedEvent(Guid OccasionId, Guid ChangedBy, DateTime ChangedAt);
    public record OccasionUpdatedEvent(Guid OccasionId, Guid ChangedBy, DateTime ChangedAt);
    public record OccasionArchivedEvent(Guid OccasionId, Guid ChangedBy, DateTime ChangedAt);

    public record HomeSectionsChangedEvent(Guid ChangedBy, DateTime ChangedAt);
}
