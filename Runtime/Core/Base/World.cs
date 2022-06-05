using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public interface IWorld
    {
        ConfigurationsRegistry ConfigurationsRegistry { get; }
        ComponentsRegistry ComponentsRegistry { get; }
        SystemsRegistry SystemsRegistry { get; }
    }

    public class World : MonoBehaviour, IWorld
    {
        public ConfigurationsRegistry ConfigurationsRegistry { get; } = new();
        public ComponentsRegistry ComponentsRegistry { get; } = new();
        public SystemsRegistry SystemsRegistry { get; } = new();
    }
}