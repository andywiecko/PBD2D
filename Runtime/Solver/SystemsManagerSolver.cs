using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Solver
{
    public class SystemsManagerSolver : MonoBehaviour
    {
        [Serializable]
        public class SerializedType2
        {
            [HideInInspector, SerializeField]
            private string tag = "";

            [field: SerializeField, HideInInspector]
            public string Guid { get; private set; } = default;

            public SerializedType2(Type t, string guid)
            {
                tag = t.Name.ToNonPascal();
                Guid = guid;
            }
        }

        [Serializable]
        public class SerializedTypeBoolTuple
        {
            public SerializedType2 type;
            public bool value = true;
        }

        [field: SerializeField]
        public World World { get; private set; } = default;

        [field: SerializeField, HideInInspector]
        public List<SerializedTypeBoolTuple> Systems { get; private set; } = new();

        private readonly Dictionary<Type, ISystem> systems = new();

        private void Awake()
        {
            foreach (var t in ISystemUtils.Types)
            {
                var s = Activator.CreateInstance(t) as ISystem;
                s.World = World;
                systems.Add(t, s);
            }
        }

        private void Start()
        {
            foreach (var (t, s) in systems)
            {
                World.SystemsRegistry.Add(s);
            }
        }

        private void OnValidate()
        {
            Systems.Clear();
            foreach (var t in ISystemUtils.Types)
            {
                SerializedTypeBoolTuple tuple = new() { type = new(t, ISystemUtils.TypeToGuid[t]) };
                Systems.Add(tuple);
            }
        }
    }
}