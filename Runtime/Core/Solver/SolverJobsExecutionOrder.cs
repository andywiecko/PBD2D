using andywiecko.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    [Serializable]
    public class SerializedType
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: SerializeField, HideInInspector]
        public string Guid { get; private set; } = default;

#if UNITY_EDITOR
        [SerializeField]
        private MonoScript script = default;
#endif

        [HideInInspector, SerializeField]
        private string assemblyQualifiedName = "";

        public Type Value => Type.GetType(assemblyQualifiedName);

        public SerializedType(Type type, string guid)
        {
            Guid = guid;
            Validate(type);
        }

        public void Validate(Type type)
        {
            tag = type.Name.ToNonPascal();
            assemblyQualifiedName = type.AssemblyQualifiedName;
#if UNITY_EDITOR
            var path = AssetDatabase.GUIDToAssetPath(Guid);
            script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
#endif
        }
    }

    [Serializable]
    public class UnconfiguredType
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: HideInInspector, SerializeField]
        public SerializedType Type { get; private set; }

        [field: SerializeField]
        public SimulationStep Step { get; private set; } = SimulationStep.Undefined;

        public UnconfiguredType(SerializedType type)
        {
            tag = type.Value.Name.ToNonPascal();
            Type = type;
        }

        public UnconfiguredType(Type type, string guid) : this(new(type, guid)) { }
    }

    [CreateAssetMenu(fileName = "SolverSystemsExecutionOrder",
        menuName = "PBD2D/Solver/Solver Systems Execution Order")]
    public class SolverJobsExecutionOrder : SolverJobsOrder
    {
        private List<Type> GetSerializedTypes() => new[] { frameStart, substep, frameEnd }
            .SelectMany(i => i)
            .Select(i => i.Value)
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
                    types.Add(st.Value);
                }
                jobsOrder.Add(s, types);
            }
        }

        private void Awake() => ValidateTypes();

        private void OnValidate() => ValidateTypes();

        private void ValidateTypes()
        {
            // HACK:
            //   For unknown reason static dicts don't survive when saving assest,
            //   but OnValidate is called during save.
            if (ISystemUtils.GuidToType.Count == 0)
            {
                return;
            }

            var typesToAssign = undefinedTypes.Where(t => t.Step != SimulationStep.Undefined);
            foreach (var u in typesToAssign)
            {
                GetListAtStep(u.Step).Add(u.Type);
            }

            foreach (var step in SystemExtensions.GetValues<SimulationStep>().Except(new[] { SimulationStep.Undefined }))
            {
                var list = GetListAtStep(step)
                    .DistinctBy(i => i.Guid)
                    .Where(i => i.Value is not null)
                    .ToList();

                list.RemoveAll(i => !ISystemUtils.GuidToType.ContainsKey(i.Guid));

                SetListAtStep(step, list);
            }

            undefinedTypes.Clear();
            foreach (var t in ISystemUtils.Types.Except(GetSerializedTypes()))
            {
                undefinedTypes.Add(new(t, ISystemUtils.TypeToGuid[t]));
            }

            foreach (var step in SystemExtensions.GetValues<SimulationStep>())
            {
                var list = GetListAtStep(step);
                if (list is not null)
                {
                    foreach (var l in list)
                    {
                        l.Validate(ISystemUtils.GuidToType[l.Guid]);
                    }
                }
            }
        }

        public override List<Func<JobHandle, JobHandle>> GenerateJobs(World world)
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();
            RegenerateJobsOrder();

            jobs.AddRange(GetJobsFor(SimulationStep.FrameStart, world));
            for (int step = 0; step < world.ConfigurationsRegistry.Get<PBDConfiguration>().StepsCount; step++)
            {
                jobs.AddRange(GetJobsFor(SimulationStep.Substep, world));
            }
            jobs.AddRange(GetJobsFor(SimulationStep.FrameEnd, world));

            return jobs;
        }

        private List<Func<JobHandle, JobHandle>> GetJobsFor(SimulationStep step, World world)
        {
            var jobs = new List<Func<JobHandle, JobHandle>>();
            foreach (var type in jobsOrder[step])
            {
                if(world.SystemsRegistry.TryGetSystem(type, out var system))
                {
                    jobs.Add(system.Schedule);
                }
            }
            return jobs;
        }
    }
}