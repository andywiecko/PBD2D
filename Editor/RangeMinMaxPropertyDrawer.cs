using andywiecko.PBD2D.Core;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Editor
{
    [CustomPropertyDrawer(typeof(RangeMinMaxAttribute))]
    public class RangeMinMaxPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUI.GetPropertyHeight(property) * 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var (min, max) = attribute as RangeMinMaxAttribute;

            switch (property.type)
            {
                case nameof(float2):
                    var xProp = property.FindPropertyRelative("x");
                    var yProp = property.FindPropertyRelative("y");
                    var x = xProp.floatValue;
                    var y = yProp.floatValue;
                    var value = Draw(position, label, x, y, min, max);
                    xProp.floatValue = value.x;
                    yProp.floatValue = value.y;
                    return;

                case nameof(Vector2):
                    var v = property.vector2Value;
                    property.vector2Value = Draw(position, label, v.x, v.y, min, max);
                    return;

                default:
                    return;
            }

            static Vector2 Draw(Rect position, GUIContent label, float x, float y, float min, float max)
            {
                var value = new Vector2(x, y);
                value = EditorGUI.Vector2Field(position, label, value);
                EditorGUI.indentLevel++;
                var indentedRect = EditorGUI.IndentedRect(
                    new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2)
                );
                EditorGUI.MinMaxSlider(position: indentedRect, minValue: ref value.x, maxValue: ref value.y, min, max);
                return value;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var (min, max) = attribute as RangeMinMaxAttribute;

            var v2field = new Vector2Field(property.name);
            v2field.tooltip = property.tooltip;
            root.Add(v2field);
            var xProperty = property.FindPropertyRelative("x");
            var yProperty = property.FindPropertyRelative("y");
            var x = xProperty.floatValue;
            var y = yProperty.floatValue;
            v2field.value = new(x, y);

            var xinput = v2field.Q("unity-x-input") as FloatField;
            xinput.Bind(xProperty.serializedObject);
            xinput.bindingPath = "x";

            var yinput = v2field.Q("unity-y-input") as FloatField;
            yinput.Bind(yProperty.serializedObject);
            yinput.bindingPath = "y";

            var slider = new MinMaxSlider(x, y, min, max);
            slider.RegisterValueChangedCallback(evt =>
            {
                v2field.value = slider.value;
                property.serializedObject.ApplyModifiedProperties();
            });
            root.Add(slider);

            v2field.RegisterValueChangedCallback(evt => slider.value = v2field.value);

            return root;
        }
    }
}