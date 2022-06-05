using andywiecko.PBD2D.Core.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class ISystemUtils
    {
        public static readonly Type[] Types;
        public static readonly IReadOnlyDictionary<Type, string> TypeToGuid;
        public static readonly IReadOnlyDictionary<string, Type> GuidToType;

        static ISystemUtils()
        {
            Types = TypeCache
                .GetTypesDerivedFrom<ISystem>().ToArray()
                .Where(s => !s.IsAbstract && s.GetCustomAttributes<FakeSystemAttribute>().Count() == 0)
                .ToArray();

            var typeToGuid = new Dictionary<Type, string>();
            var guidToType = new Dictionary<string, Type>();

            void RegisterMapping(Type type, string guid)
            {
                typeToGuid.Add(type, guid);
                guidToType.Add(guid, type);
            }

            foreach (var type in Types)
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

            TypeToGuid = typeToGuid;
            GuidToType = guidToType;
        }
    }
}
