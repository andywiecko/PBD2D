#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Core.Editor
{
    [CustomEditor(typeof(SystemsManager))]
    public class SystemsManagerEditor : UnityEditor.Editor
    {
        private static readonly IReadOnlyDictionary<string, Dictionary<Type, string>> categoryToTypes;

        private SystemsManager Target => target as SystemsManager;

        private PrefabStage prefabStage;
        private (bool isPrefabInstance, bool isPrefabAsset, bool isStage) targetStatus;

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

        private void OnEnable()
        {
            targetStatus.isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target);
            targetStatus.isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target);
            prefabStage = PrefabStageUtility.GetPrefabStage(Target.gameObject);
            targetStatus.isStage = prefabStage != null;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            if (targetStatus.isPrefabAsset)
            {
                root.Add(new HelpBox("Editing in asset view is not supported.\nPlease open prefab in isolation mode.", HelpBoxMessageType.Warning));
            }
            else
            {
                root.Add(new IMGUIContainer(base.OnInspectorGUI));
                root.Add(SystemsList());
            }

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
            if (categoryComponent == null)
            {
                categoryComponent = new GameObject(category).transform;
                categoryComponent.transform.parent = Target.transform;
            }

            var value = categoryComponent.GetComponent(type) != null;
            var toggle = new Toggle(name) { value = value };
            toggle.RegisterValueChangedCallback((evt) =>
            {
                switch (targetStatus)
                {
                    case (false, false, false):
                        NonPrefabInstanceCase(evt.newValue, categoryComponent, type);
                        break;

                    case (true, false, false):
                        PrefabInstanceCase(evt.newValue, categoryComponent, type);
                        break;

                    case (false, false, true):
                        PrefabAssetIsolationStageCase(evt.newValue, categoryComponent, type);
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

        private void NonPrefabInstanceCase(bool value, Transform category, Type type)
        {
            switch (value)
            {
                case true:
                    Undo.AddComponent(category.gameObject, type);
                    break;

                case false:
                    Undo.DestroyObjectImmediate(category.GetComponent(type));
                    break;
            }
        }

        private void PrefabInstanceCase(bool value, Transform category, Type type)
        {
            switch (value)
            {
                case true:
                    var removedComponent = PrefabUtility
                        .GetRemovedComponents(category.gameObject)
                        .FirstOrDefault(c => c.assetComponent.GetType() == type);

                    if (removedComponent != null)
                    {
                        removedComponent.Revert();
                    }
                    else
                    {
                        Undo.AddComponent(category.gameObject, type);
                    }

                    break;

                case false:
                    Undo.DestroyObjectImmediate(category.GetComponent(type));
                    break;
            }
        }

        private void PrefabAssetIsolationStageCase(bool value, Transform category, Type type)
        {
            switch (value)
            {
                case true:
                    Undo.AddComponent(category.gameObject, type);
                    break;

                case false:
                    Undo.DestroyObjectImmediate(category.GetComponent(type));
                    break;
            }

            EditorUtility.SetDirty(target);
        }
    }
}
#endif