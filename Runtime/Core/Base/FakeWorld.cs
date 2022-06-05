namespace andywiecko.PBD2D.Core
{
    // Move this to test system assembly
    public class FakeWorld : IWorld
    {
        public SimulationConfiguration Configuration { get; set; }

        public ComponentsRegistry ComponentsRegistry { get; } = new();
        public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
        public SystemsRegistry SystemsRegistry { get; } = new();

        public FakeWorld(params IComponent[] components)
        {
            foreach (var c in components)
            {
                ComponentsRegistry.Add(c);
            }
        }
    }
}