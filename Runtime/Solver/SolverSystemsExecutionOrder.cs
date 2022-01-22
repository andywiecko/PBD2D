using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Core.Editor;
using andywiecko.PBD2D.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D
{
    [Serializable]
    public class SerializedType
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: SerializeField, HideInInspector]
        public string Guid { get; private set; } = default;

        [SerializeField]
        private string assembly = "";

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
            assembly = type.Assembly.FullName;
            assemblyQualifiedName = type.AssemblyQualifiedName;
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
    public class SolverSystemsExecutionOrder : ScriptableObject
    {
        private static Type[] types;
        private static readonly Dictionary<Type, string> typeToGuid = new();
        private static readonly Dictionary<string, Type> guidToType = new();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            types = TypeCache
                .GetTypesDerivedFrom<ISystem>().ToArray()
                .Where(s => !s.IsAbstract)
                .ToArray();

            static void RegisterMapping(Type type, string guid)
            {
                typeToGuid.Add(type, guid);
                guidToType.Add(guid, type);
            }

            foreach (var type in types)
            {
                var guid = AssetDatabaseUtils.TryGetTypeGUID(type);

                if (guid is string)
                {
                    RegisterMapping(type, guid);
                }
                else
                {
                    Debug.Log($"Error {type}"); // throw something
                }
            }
        }

        private List<Type> GetSerializedTypes() => new[] { frameStart, stepStart, subStep, stepEnd, frameEnd }
            .SelectMany(i => i)
            .Select(i => i.Value)
            .ToList();

        private List<SerializedType> GetListAtStep(SimulationStep step) => step switch
        {
            SimulationStep.FrameStart => frameStart,
            SimulationStep.StepStart => stepStart,
            SimulationStep.SubStep => subStep,
            SimulationStep.StepEnd => stepEnd,
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

                case SimulationStep.StepStart:
                    stepStart = list;
                    break;

                case SimulationStep.SubStep:
                    subStep = list;
                    break;

                case SimulationStep.StepEnd:
                    stepEnd = list;
                    break;

                case SimulationStep.FrameEnd:
                    frameEnd = list;
                    break;
            }
        }

        [SerializeField] private List<SerializedType> frameStart = new();
        [SerializeField] private List<SerializedType> stepStart = new();
        [SerializeField] private List<SerializedType> subStep = new();
        [SerializeField] private List<SerializedType> stepEnd = new();
        [SerializeField] private List<SerializedType> frameEnd = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredType> undefinedTypes = new();

        private void OnValidate()
        {
            // HACK:
            //   For unknown reason static dicts don't survive when saving assest,
            //   but OnValidate is called during save.
            if (guidToType.Count == 0)
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

                list.RemoveAll(i => !guidToType.ContainsKey(i.Guid));

                SetListAtStep(step, list);
            }

            undefinedTypes.Clear();
            foreach (var t in types.Except(GetSerializedTypes()))
            {
                undefinedTypes.Add(new(t, typeToGuid[t]));
            }
        }
    }
}