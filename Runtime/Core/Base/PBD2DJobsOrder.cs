using andywiecko.ECS;
#if UNITY_EDITOR
using andywiecko.ECS.Editor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    [CreateAssetMenu(fileName = "PBD2D Jobs Order", menuName = "PBD2D/Solver/PBD2D Jobs Order")]
    public class PBD2DJobsOrder : JobsOrder
    {
        private enum SimulationStep
        {
            Undefined = -1,
            FrameStart,
            Substep,
            FrameEnd
        }

        [Serializable]
        private class UnconfiguredType
        {
            [HideInInspector, SerializeField]
            private string tag = "";

            [field: SerializeField]
            public SerializedType Type { get; private set; }

            [field: SerializeField]
            public SimulationStep Step { get; private set; } = SimulationStep.Undefined;

            public UnconfiguredType(Type type, string guid)
            {
                Type = new(type, guid);
                tag = type.Name.ToNonPascal();
            }
        }

        [field: SerializeField, HideInInspector]
        public string[] TargetAssemblies { get; private set; } = { };
#if UNITY_EDITOR
        private IEnumerable<Type> TargetTypes => TypeCacheUtils.Systems.GetTypes(TargetAssemblies);
        [SerializeField]
        private UnityEditorInternal.AssemblyDefinitionAsset[] targetAssemblies = { };
#endif

        [Space(50)]
        [SerializeField] private List<SerializedType> frameStart = new();
        [SerializeField] private List<SerializedType> substep = new();
        [SerializeField] private List<SerializedType> frameEnd = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredType> undefinedTypes = new();

#if UNITY_EDITOR
        private void Awake() => ValidateSerializedTypes();

        private void OnValidate()
        {
            TargetAssemblies = targetAssemblies?.GetNames().ToArray();

            // HACK:
            //   For unknown reason static dicts don't survive when saving asset,
            //   but OnValidate is called during save.
            if (TypeCacheUtils.Systems.GuidToType.Count != 0)
            {
                ValidateSerializedTypes();
            }
        }

        private void ValidateSerializedTypes()
        {
            RemoveBadTypes();
            AssignEnabledTypes();
            FillUndefinedTypes();
            ValidateEnabledTypes();
        }

        private void RemoveBadTypes()
        {
            frameStart.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Guid));
            substep.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Guid));
            frameEnd.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Guid));
            undefinedTypes.RemoveAll(i => !TypeCacheUtils.Systems.GuidToType.ContainsKey(i.Type.Guid));

            frameStart = frameStart.DistinctBy(i => i.Guid).ToList();
            substep = substep.DistinctBy(i => i.Guid).ToList();
            frameEnd = frameEnd.DistinctBy(i => i.Guid).ToList();
        }

        private void AssignEnabledTypes()
        {
            var typesToAssign = undefinedTypes.Where(t => t.Step != SimulationStep.Undefined);
            foreach (var u in typesToAssign)
            {
                var list = u.Step switch
                {
                    SimulationStep.FrameStart => frameStart,
                    SimulationStep.Substep => substep,
                    SimulationStep.FrameEnd => frameEnd,
                    _ => throw new Exception()
                };

                list.Add(u.Type);
            }
        }

        private void FillUndefinedTypes()
        {
            undefinedTypes.Clear();

            var enabledTypes = frameStart.Concat(substep).Concat(frameEnd);

            foreach (var t in TargetTypes)
            {
                if (!enabledTypes.Select(i => i.Type).Contains(t) &&
                    TypeCacheUtils.Systems.TypeToGuid.TryGetValue(t, out var guid))
                {
                    undefinedTypes.Add(new(t, guid));
                }
            }
        }

        private void ValidateEnabledTypes()
        {
            var enabledTypes = frameStart.Concat(substep).Concat(frameEnd);
            foreach (var t in enabledTypes)
            {
                t.Validate(TypeCacheUtils.Systems.GuidToType[t.Guid]);
            }
        }
#endif

        public override void GenerateJobs(ISolver solver, IWorld world)
        {
            var jobs = solver.Jobs;
            var systems = world.SystemsRegistry;

            foreach (var type in frameStart.Select(i => i.Type))
            {
                if (systems.TryGetSystem(type, out var system))
                {
                    jobs.Add(system.Schedule);
                }
            }

            var stepsCount = world.ConfigurationsRegistry.Get<PBDConfiguration>().StepsCount;
            for (int step = 0; step < stepsCount; step++)
            {
                foreach (var type in substep.Select(i => i.Type))
                {
                    if (systems.TryGetSystem(type, out var system))
                    {
                        jobs.Add(system.Schedule);
                    }
                }
            }

            foreach (var type in frameEnd.Select(i => i.Type))
            {
                if (systems.TryGetSystem(type, out var system))
                {
                    jobs.Add(system.Schedule);
                }
            }
        }
    }
}