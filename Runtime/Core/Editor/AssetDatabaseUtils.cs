#if UNITY_EDITOR
using System;
using UnityEditor;

namespace andywiecko.PBD2D.Core.Editor
{
    public static class AssetDatabaseUtils
    {
        public static string TryGetTypeGUID(Type type)
        {
            string guid = default;
            var typeName = type.Name;
            var guids = AssetDatabase.FindAssets($"{typeName} t:script");

            if (guids.Length == 1)
            {
                guid = guids[0];
            }
            else
            {
                foreach (var guidToCheck in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guidToCheck);
                    var split = path.Split("/")[^1].Split(".cs");
                    if (split.Length == 2 && split[0] == type.Name)
                    {
                        guid = guidToCheck;
                        break;
                    }
                }
            }

            return guid;
        }
    }
}
#endif