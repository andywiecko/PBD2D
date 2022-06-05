namespace andywiecko.PBD2D.Core
{
    public abstract class BaseSystemWithConfiguration<TComponent, TConfiguration> : BaseSystem<TComponent>
        where TComponent : IComponent
        where TConfiguration : class, IConfiguration
    {
        protected TConfiguration Configuration => World.ConfigurationsRegistry.Get<TConfiguration>();
    }
}