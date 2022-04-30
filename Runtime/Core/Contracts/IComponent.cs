using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public interface IEntity
    {
        Id<IEntity> Id { get; }
    }

    public interface IComponent
    {
        // TODO: rename this into ComponentId
        Id<IComponent> Id { get; }
    }

    public interface IEntityComponent : IComponent
    {
        Id<IEntity> EntityId { get; }
    }
}