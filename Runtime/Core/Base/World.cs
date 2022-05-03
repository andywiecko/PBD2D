using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface IWorld
    {
        SimulationConfiguration Configuration { get; }
        ComponentsRegistry ComponentsRegistry { get; }
        SystemsRegistry SystemsRegistry { get; }
    }

    public class World : MonoBehaviour, IWorld
    {
        public SimulationConfiguration Configuration { get; set; }
        public ComponentsRegistry ComponentsRegistry { get; } = new();
        public SystemsRegistry SystemsRegistry { get; } = new();
    }
}