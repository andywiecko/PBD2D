using andywiecko.PBD2D.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Solver.Editor
{
    [CustomEditor(typeof(SystemsManagerSolver))]
    public class SystemsManagerSolverEditor : UnityEditor.Editor
    {
        private static readonly IReadOnlyDictionary<string, Dictionary<Type, string>> categoryToTypes;
        private static readonly IReadOnlyDictionary<Type, string> typesToCategory;

        static SystemsManagerSolverEditor()
        {
            var types = TypeCache.GetTypesDerivedFrom<ISystem>()
                .Where(s => s.IsGenericType == false && s.IsAbstract == false);
            var tmp = new Dictionary<string, Dictionary<Type, string>>();
            var tmp3 = new Dictionary<Type, string>();
            foreach (var type in types)
            {
                var name = type.GetCustomAttribute<AddComponentMenu>()?.componentMenu ?? type.Name;
                var subnames = name.Split('/');

                var typeName = subnames.Last();
                var categoryName = subnames.Length >= 3 ? subnames[subnames.Length - 2] : "Others";
                if (!tmp.TryGetValue(categoryName, out var typesToNames))
                {
                    typesToNames = new Dictionary<Type, string>();
                    tmp.Add(categoryName, typesToNames);
                }

                typesToNames.Add(type, typeName);
                tmp3.Add(type, categoryName);
            }
            categoryToTypes = tmp;
            typesToCategory = tmp3;
        }

        private SystemsManagerSolver Target => target as SystemsManagerSolver;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new IMGUIContainer(base.OnInspectorGUI));

            var list = categoryToTypes.Keys.ToDictionary(i => i, i =>
            {
                var foldout = new Foldout()
                {
                    text = i,
                    style = {
                        color = new StyleColor(new Color(.678f, .847f, .902f)) ,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                    }
                };

                return foldout;
            });

            foreach (var tuple in Target.Systems)
            {
                var line = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row ,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                    }
                };
                var toggle = new Toggle()
                {
                    value = tuple.value,
                    style =
                    {
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal),
                        alignSelf = new StyleEnum<Align>(Align.FlexStart)
                    }
                };

                toggle.RegisterValueChangedCallback(evt =>
                {
                    tuple.value = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                });

                var type = ISystemUtils.GuidToType[tuple.type.Guid];
                var category = typesToCategory[type];
                var element = list[category];
                line.Add(toggle);

                var path = AssetDatabase.GUIDToAssetPath(tuple.type.Guid);
                var scriptField = new ObjectField()
                {
                    value = AssetDatabase.LoadAssetAtPath<MonoScript>(path),
                    objectType = typeof(MonoScript),
                };

                //scriptField.Q<Label>().text = type.Name.ToNonPascal();

                scriptField.Q(className: ObjectField.selectorUssClassName).SetEnabled(false);

                line.Add(scriptField);

                element.Add(line);
            }

            foreach (var (i, ve) in list)
            {
                root.Add(ve);
            }

            return root;
        }
    }
}