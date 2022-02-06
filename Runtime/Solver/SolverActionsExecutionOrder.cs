using andywiecko.PBD2D.Core;
using andywiecko.PBD2D.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Solver
{
    public enum SolverAction
    {
        Undefined = -1,
        OnScheduling,
        OnJobsCompletion
    }

    [Serializable]
    public class SerializedMethod
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: HideInInspector, SerializeField]
        public string Name { get; private set; } = "";

        [field: HideInInspector, SerializeField]
        public SerializedType SerializedType { get; private set; } = default;

        [SerializeField]
        private string assembly = "";

        public (MethodInfo MethodInfo, Type Type) Value => (SerializedType.Value.GetMethod(Name), SerializedType.Value);

        public void Deconstruct(out MethodInfo m, out Type t) => (m, t) = Value;

        public SerializedMethod(MethodInfo methodInfo, Type type, string guid)
        {
            Name = methodInfo.Name;
            tag = Name.ToNonPascal() + $" ({type.Name.ToNonPascal()})";
            this.SerializedType = new(type, guid);
            Validate(type);
        }

        public void Validate(Type type)
        {
            this.SerializedType.Validate(type);
            assembly = type.Assembly.FullName;
        }
    }

    [Serializable]
    public class UnconfiguredMethod
    {
        [HideInInspector, SerializeField]
        private string tag = "";

        [field: HideInInspector, SerializeField]
        public SerializedMethod Method { get; private set; }

        [field: SerializeField]
        public SolverAction Action { get; private set; } = SolverAction.Undefined;

        public UnconfiguredMethod(SerializedMethod method)
        {
            var (m, t) = method.Value;
            tag = m.Name.ToNonPascal() + $" ({t.Name.ToNonPascal()})";
            Method = method;
        }

        public UnconfiguredMethod(MethodInfo method, Type type, string guid) : this(new(method, type, guid)) { }
    }

    [CreateAssetMenu(fileName = "SolverActionsExecutionOrder",
        menuName = "PBD2D/Solver/Solver Actions Execution Order")]
    public class SolverActionsExecutionOrder : ScriptableObject
    {
        private static readonly Dictionary<MethodInfo, Type> methodToType = new();
        private static readonly Dictionary<Type, string> typeToGuid = new();
        private static readonly Dictionary<string, Type> guidToType = new();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var methods = TypeCache
                .GetMethodsWithAttribute<SolverActionAttribute>()
                .ToArray();

            foreach (var m in methods)
            {
                var t = m.ReflectedType;
                methodToType.Add(m, t);
            }

            var types = methodToType.Values.Distinct();

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
                    throw new NotImplementedException("This Type-GUID case is not handled yet.");
                }

            }
        }
#endif

        private List<SerializedMethod> GetListAtAction(SolverAction action) => action switch
        {
            SolverAction.OnScheduling => onScheduling,
            SolverAction.OnJobsCompletion => onJobsCompletion,
            _ => default,
        };

        private void SetListAtAction(SolverAction action, List<SerializedMethod> list)
        {
            switch (action)
            {
                case SolverAction.OnScheduling:
                    onScheduling = list;
                    break;

                case SolverAction.OnJobsCompletion:
                    onJobsCompletion = list;
                    break;
            }
        }

        private readonly Dictionary<SolverAction, List<(MethodInfo, Type)>> actionOrder = new();

        public IReadOnlyDictionary<SolverAction, List<(MethodInfo, Type)>> GetActionOrder()
        {
            actionOrder.Clear();
            foreach (var a in SystemExtensions.GetValues<SolverAction>())
            {
                var list = GetListAtAction(a);
                if (list is null) continue;
                var methods = new List<(MethodInfo, Type)>(capacity: list.Count);
                foreach(var (m, t) in list)
                {
                    methods.Add((m, t));
                }
                actionOrder.Add(a, methods);
            }

            return actionOrder;
        }

        private List<(MethodInfo, Type)> GetSerializedMethods() => new[] { onScheduling, onJobsCompletion }
            .SelectMany(i => i)
            .Select(i => i.Value)
            .ToList();

        [SerializeField] private List<SerializedMethod> onScheduling = new();
        [SerializeField] private List<SerializedMethod> onJobsCompletion = new();

        [Space(30)]
        [SerializeField]
        private List<UnconfiguredMethod> undefinedMethods = new();

        private void OnValidate()
        {
            // HACK:
            //   For unknown reason static dicts don't survive when saving assest,
            //   but OnValidate is called during save.
            if (guidToType.Count == 0)
            {
                return;
            }

            var methodsToAssign = undefinedMethods.Where(t => t.Action != SolverAction.Undefined);
            foreach (var u in methodsToAssign)
            {
                GetListAtAction(u.Action).Add(u.Method);
            }

            foreach (var action in SystemExtensions.GetValues<SolverAction>().Except(new[] { SolverAction.Undefined }))
            {
                var list = GetListAtAction(action)
                    .DistinctBy(i => (i.Name, i.SerializedType.Guid))
                    .Where(i => i is not null)
                    .ToList();

                list.RemoveAll(i => !guidToType.ContainsKey(i.SerializedType.Guid));

                SetListAtAction(action, list);
            }

            undefinedMethods.Clear();
            var serializedMethods = GetSerializedMethods();
            foreach (var (m, t) in methodToType)
            {
                if (!serializedMethods.Contains((m, t)))
                {
                    undefinedMethods.Add(new(m, t, typeToGuid[t]));
                }
            }
        }
    }
}