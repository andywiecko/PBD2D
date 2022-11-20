using andywiecko.PBD2D.Components;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Editor
{
    [CustomEditor(typeof(PointsLockerGenerator), editorForChildClasses: true)]
    public class PointsLockerGeneratorEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imgui = new IMGUIContainer(base.OnInspectorGUI) { name = "imgui" };
            root.Add(imgui);

            var stiffness = serializedObject.FindProperty("<Stiffness>k__BackingField");
            var compliance = serializedObject.FindProperty("<Compliance>k__BackingField");
            var type = serializedObject.FindProperty("type");
            var regenerateOnChange = serializedObject.FindProperty("regenerateOnChange");

            var stiffnessProperty = new PropertyField(stiffness);
            var complianceProperty = new PropertyField(compliance);
            var typeProperty = new PropertyField(type);
            var regenerateOnChangeProperty = new PropertyField(regenerateOnChange);
            var helpBox = new HelpBox("Warning: Hard locks support is experimental!", HelpBoxMessageType.Warning);

            typeProperty.SetEnabled(!Application.isPlaying);
            regenerateOnChangeProperty.SetEnabled(!Application.isPlaying);

            root.Add(typeProperty);
            root.Add(stiffnessProperty);
            root.Add(complianceProperty);
            root.Add(regenerateOnChangeProperty);
            root.Add(helpBox);

            ResetFields();
            typeProperty.RegisterValueChangeCallback(_ =>
            {
                ResetFields();
            });

            void ResetFields()
            {
                var typeValue = type.intValue;
                if (typeValue is not (0 or 1))
                {
                    throw new NotImplementedException();
                }

                var style = (DisplayStyle)typeValue;
                var otherStyle = (DisplayStyle)(typeValue ^ 1);

                helpBox.style.display = otherStyle;
                stiffnessProperty.style.display = style;
                complianceProperty.style.display = style;
            }

            return root;
        }
    }
}