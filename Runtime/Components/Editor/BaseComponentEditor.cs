using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        private PrefabStage prefabStage;
        private (bool isPrefabInstance, bool isPrefabAsset, bool isStage) targetStatus;

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

            if (targetStatus.isPrefabAsset)
            {
                root.Add(new HelpBox("Editing in asset view is not supported.\nPlease open prefab in isolation mode.", HelpBoxMessageType.Warning));
            }
            else
            {
                root.Add(ComponentsList());
            }

            return root;
        }

        protected VisualElement ComponentsList()
        {
            var root = new VisualElement();
            root.Add(new Label());
            var components = new Foldout()
            {
                text = "Components",
                style = { unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold) }
            };
            root.Add(components);

            foreach (var (category, typeTonNames) in categoryToTypes)
            {
                var foldout = new Foldout()
                {
                    text = category,
                    style = { color = new StyleColor(new Color(.678f, .847f, .902f)) }
                };

                foreach (var (type, name) in typeTonNames)
                {
                    var toggle = CreateToggleButtonForType(type, name);
                    foldout.Add(toggle);
                }

                components.Add(foldout);
            }

            return root;
        }

        protected VisualElement CreateToggleButtonForType(Type type, string name)
        {
            var value = Target.GetComponent(type) != null;
            var toggle = new Toggle(name)
            {
                value = value,
                style = { unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal) }
            };
            toggle.RegisterValueChangedCallback((evt) =>
            {
                switch (targetStatus)
                {
                    case (false, false, false):
                        NonPrefabInstanceCase(evt.newValue, type);
                        break;

                    case (true, false, false):
                        PrefabInstanceCase(evt.newValue, type);
                        break;

                    case (false, false, true):
                        PrefabAssetIsolationStageCase(evt.newValue, type);
                        break;

                    case (false, true, false):
                        // TODO: PrefabAssetNonIsolationStageCase
                        break;

                    default:
                        throw new NotImplementedException();
                }



            });
            return toggle;
        }

        private void PrefabAssetIsolationStageCase(bool newValue, Type type)
        {
            switch (newValue)
            {
                case true:
                    Undo.AddComponent(Target.gameObject, type);
                    break;

                case false:
                    Undo.DestroyObjectImmediate(Target.GetComponent(type));
                    break;
            }

            EditorUtility.SetDirty(target);
        }

        private void PrefabInstanceCase(bool value, Type type)
        {
            switch (value)
            {
                case true:
                    var removedComponent = PrefabUtility
                        .GetRemovedComponents(Target.gameObject)
                        .FirstOrDefault(c => c.assetComponent.GetType() == type);

                    if (removedComponent != null)
                    {
                        removedComponent.Revert();
                    }
                    else
                    {
                        Undo.AddComponent(Target.gameObject, type);
                    }

                    break;

                case false:
                    Undo.DestroyObjectImmediate(Target.GetComponent(type));
                    break;
            }
        }

        private void NonPrefabInstanceCase(bool newValue, Type type)
        {
            switch (newValue)
            {
                case true:
                    Undo.AddComponent(Target.gameObject, type);
                    break;

                case false:
                    Undo.DestroyObjectImmediate(Target.GetComponent(type));
                    break;
            }
        }

        private void OnEnable()
        {
            RefreshTargetStatus();
        }

        private void OnValidate()
        {
            RefreshTargetStatus();
        }

        private void RefreshTargetStatus()
        {
            targetStatus.isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target);
            targetStatus.isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target);
            prefabStage = PrefabStageUtility.GetPrefabStage(Target.gameObject);
            targetStatus.isStage = prefabStage != null;
        }
    }
}
