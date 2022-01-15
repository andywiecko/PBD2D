using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Core.Editor
{
    [CustomEditor(typeof(SystemsManager))]
    public class SystemsManagerEditor : UnityEditor.Editor
    {
        private static readonly IReadOnlyDictionary<string, Dictionary<Type, string>> categoryToTypes;

        private SystemsManager Target => target as SystemsManager;

        static SystemsManagerEditor()
        {
            var types = TypeCache.GetTypesDerivedFrom<ISystem>()
                .Where(s => s.IsGenericType == false && s.IsAbstract == false);
            var tmp = new Dictionary<string, Dictionary<Type, string>>();

            foreach (var type in types)
            {
                var name = (type.GetCustomAttribute(typeof(AddComponentMenu)) as AddComponentMenu)?.componentMenu ?? type.Name;
                var subnames = name.Split('/');

                var typeName = subnames.Last();
                var categoryName = subnames.Length >= 3 ? subnames[subnames.Length - 2] : "Others";
                if (!tmp.TryGetValue(categoryName, out var typesToNames))
                {
                    typesToNames = new Dictionary<Type, string>();
                    tmp.Add(categoryName, typesToNames);
                }

                typesToNames.Add(type, typeName);
            }
            categoryToTypes = tmp;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new IMGUIContainer(base.OnInspectorGUI));
            root.Add(SystemsList());

            return root;
        }

        protected VisualElement SystemsList()
        {
            var root = new VisualElement();
            foreach (var (category, typeTonNames) in categoryToTypes)
            {
                var label = new Label(category);
                label.style.color = new StyleColor(new Color(.678f, .847f, .902f));
                root.Add(label);

                foreach (var (type, name) in typeTonNames)
                {
                    var toggle = CreateToggleButtonForType(type, category, name);
                    root.Add(toggle);
                }
            }

            return root;
        }

        protected VisualElement CreateToggleButtonForType(Type type, string category, string name)
        {
            var categoryComponent = Target.transform.Find(category);
            if(categoryComponent == null)
            {
                categoryComponent = new GameObject(category).transform;
                categoryComponent.transform.parent = Target.transform;
            }

            var value = categoryComponent.GetComponent(type) != null;
            var toggle = new Toggle(name) { value = value };
            toggle.RegisterValueChangedCallback((evt) =>
            {
                switch (evt.newValue)
                {
                    case true:
                        Undo.AddComponent(categoryComponent.gameObject, type);
                        break;

                    case false:
                        DestroyImmediate(categoryComponent.GetComponent(type));
                        break;
                }

            });
            return toggle;
        }
    }
}