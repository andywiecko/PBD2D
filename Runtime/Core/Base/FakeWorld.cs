using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    // Move this to test system assembly
    public class FakeWorld : IWorld
    {
        public IComponentsRegistry ComponentsRegistry { get; } = new ComponentsRegistry();
        public SimulationConfiguration Configuration { get; } = new();
        public ISystemsRegistry SystemsRegistry { get; } = new SystemsRegistry();

        ISimulationConfiguration IWorld.Configuration => Configuration;

        public FakeWorld(params object[] components)
        {
            foreach (var c in components)
            {
                ComponentsRegistry.Register(c);
            }
        }
    }
}