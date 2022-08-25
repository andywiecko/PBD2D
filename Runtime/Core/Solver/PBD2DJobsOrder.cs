using andywiecko.ECS;
using andywiecko.ECS.Editor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Jobs;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

        private List<Type> GetSerializedTypes() => new[] { frameStart, substep, frameEnd }
            .SelectMany(i => i)
            .Select(i => i.Type)
            .ToList();

        private List<SerializedType> GetListAtStep(SimulationStep step) => step switch
        {
            SimulationStep.FrameStart => frameStart,
            SimulationStep.Substep => substep,
            SimulationStep.FrameEnd => frameEnd,
            _ => default,
        };

        private void SetListAtStep(SimulationStep step, List<SerializedType> list)
        {
            switch (step)
            {
                case SimulationStep.FrameStart:
                    frameStart = list;
                    break;

                case SimulationStep.Substep:
                    substep = list;
                    break;

                case SimulationStep.FrameEnd:
                    frameEnd = list;
                    break;
            }
        }

        [field: SerializeField, HideInInspector]
        public string[] TargetAssemblies { get; private set; } = { };

#if UNITY_EDITOR
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

        private readonly Dictionary<SimulationStep, List<Type>> jobsOrder = new();

        public void RegenerateJobsOrder()
        {
            jobsOrder.Clear();

            foreach (var s in SystemExtensions.GetValues<SimulationStep>())
            {
                var list = GetListAtStep(s);
                if (list is null) continue;
                var types = new List<Type>(capacity: list.Count);
                foreach (var st in list)
                {
                    types.Add(st.Type);
                }
                jobsOrder.Add(s, types);
            }
        }

        private void Awake() => ValidateTypes();

        private void OnValidate()
        {
            TargetAssemblies = targetAssemblies?
                .Where(i => i != null)
                .Select(i => JObject.Parse(i.text)["name"].ToString()).ToArray();

            ValidateTypes();
        }

        private void ValidateTypes()
        {
            // HACK:
            //   For unknown reason static dicts don't survive when saving assest,
            //   but OnValidate is called during save.
            if (TypeCacheUtils.Systems.GuidToType.Count == 0)
            {
                return;
            }

            var targetTypes = TargetAssemblies
                .Select(i => Assembly.Load(i))
                .SelectMany(i => TypeCacheUtils.Systems.AssemblyToTypes[i]);
            var targetGuids = targetTypes.Select(i => TypeCacheUtils.Systems.TypeToGuid[i]);

            var typesToAssign = undefinedTypes.Where(t => t.Step != SimulationStep.Undefined);
            foreach (var u in typesToAssign)
            {
                GetListAtStep(u.Step).Add(u.Type);
            }

            foreach (var step in SystemExtensions.GetValues<SimulationStep>().Except(new[] { SimulationStep.Undefined }))
            {
                var list = GetListAtStep(step)
                    .DistinctBy(i => i.Guid)
                    .Where(i => i.Type is not null)
                    .ToList();

                list.RemoveAll(i => !targetGuids.Contains(i.Guid));

                SetListAtStep(step, list);
            }

            undefinedTypes.Clear();
            foreach (var t in targetTypes.Except(GetSerializedTypes()))
            {
                undefinedTypes.Add(new(t, TypeCacheUtils.Systems.TypeToGuid[t]));
            }

            foreach (var step in SystemExtensions.GetValues<SimulationStep>())
            {
                var list = GetListAtStep(step);
                if (list is not null)
                {
                    foreach (var l in list)
                    {
                        l.Validate(TypeCacheUtils.Systems.GuidToType[l.Guid]);
                    }
                }
            }
        }

        public override void GenerateJobs(ISolver solver, IWorld world)
        {
            RegenerateJobsOrder();

            var jobs = solver.Jobs;
            jobs.AddRange(GetJobsFor(SimulationStep.FrameStart, world));
            for (int step = 0; step < world.ConfigurationsRegistry.Get<PBDConfiguration>().StepsCount; step++)
            {
                jobs.AddRange(GetJobsFor(SimulationStep.Substep, world));
            }
            jobs.AddRange(GetJobsFor(SimulationStep.FrameEnd, world));
        }

        private List<Func<JobHandle, JobHandle>> GetJobsFor(SimulationStep step, IWorld world)
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();
            foreach (var type in jobsOrder[step])
            {
                if (world.SystemsRegistry.TryGetSystem(type, out var system))
                {
                    jobs.Add(system.Schedule);
                }
            }
            return jobs;
        }
    }
}