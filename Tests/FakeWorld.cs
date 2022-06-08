using andywiecko.ECS;
using andywiecko.PBD2D.Core;

namespace andywiecko.PBD2D.Editor.Tests
{
    public class FakeWorld : IWorld
    {
        public PBDConfiguration Configuration { get; } = new();
        public ComponentsRegistry ComponentsRegistry { get; } = new();
        public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
        public SystemsRegistry SystemsRegistry { get; } = new();

        public FakeWorld(params IComponent[] components)
        {
            foreach (var c in components)
            {
                ComponentsRegistry.Add(c);
            }

            ConfigurationsRegistry.Set(Configuration);
        }
    }
}