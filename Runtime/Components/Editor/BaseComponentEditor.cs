using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components.Editor
{
    public abstract class BaseComponentEditor<T> : UnityEditor.Editor
        where T : MonoBehaviour
    {
        protected T Target => target as T;
        private static readonly Type TargetType = typeof(T);
        private static readonly IReadOnlyDictionary<string, Dictionary<Type, string>> categoryToTypes;

        static BaseComponentEditor()
        {
            var types = TypeCache
                .GetTypesWithAttribute(typeof(RequireComponent))
                .Where(t => Attribute
                    .GetCustomAttributes(t, typeof(RequireComponent))
                    .Any(a => a is RequireComponent rc &&
                        (rc.m_Type0 == TargetType ||
                         rc.m_Type1 == TargetType ||
                         rc.m_Type2 == TargetType)));

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
            root.Add(ComponentsList());

            return root;
        }

        protected VisualElement ComponentsList()
        {
            var root = new VisualElement();
            foreach (var (category, typeTonNames) in categoryToTypes)
            {
                var label = new Label(category);
                label.style.color = new StyleColor(new Color(.678f, .847f, .902f));
                root.Add(label);

                foreach (var (type, name) in typeTonNames)
                {
                    var toggle = CreateToggleButtonForType(type, name);
                    root.Add(toggle);
                }
            }

            return root;
        }

        protected VisualElement CreateToggleButtonForType(Type type, string name)
        {
            var value = Target.GetComponent(type) != null;
            var toggle = new Toggle(name) { value = value };
            toggle.RegisterValueChangedCallback((evt) =>
            {
                switch (evt.newValue)
                {
                    case true:
                        Undo.AddComponent(Target.gameObject, type);
                        break;

                    case false:
                        DestroyImmediate(Target.GetComponent(type));
                        break;
                }
            });
            return toggle;
        }
    }
}
