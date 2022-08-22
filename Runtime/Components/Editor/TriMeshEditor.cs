using andywiecko.ECS.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMesh))]
    public class TriMeshEditor : EntityEditor
    {
        private Label label;
        private TriMesh triMesh;

        private void OnEnable()
        {
            label = CreateLabel();
            UpdateLabel();
            triMesh = target as TriMesh;
            triMesh.OnSerializedDataChange += UpdateLabel;
        }

        private void OnDisable()
        {
            triMesh.OnSerializedDataChange -= UpdateLabel;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();

            var serializedData = serializedObject.FindProperty("<SerializedData>k__BackingField");
            var serializedDataProperty = new PropertyField(serializedData);
            serializedDataProperty.SetEnabled(!Application.isPlaying);
            root.Insert(1, serializedDataProperty);

            root.Insert(2, label);

            return root;
        }

        private Label CreateLabel() => new()
        {
            style =
                {
                    borderBottomColor = new StyleColor(Color.white), borderBottomWidth = 1,
                    borderLeftColor = new StyleColor(Color.white), borderLeftWidth = 1,
                    borderRightColor = new StyleColor(Color.white), borderRightWidth = 1,
                    borderTopColor = new StyleColor(Color.white), borderTopWidth = 1,
                    marginTop = 15, marginBottom = 15,
                    paddingTop = 5, paddingBottom = 5, fontSize = 13
                }
        };

        private void UpdateLabel()
        {
            var data = (target as TriMesh).SerializedData;
            var points = data == null ? "???" : data.Positions.Length.ToString();
            var triangles = data == null ? "???" : (data.Triangles.Length / 3).ToString();

            label.text = $" Data:  ● Points: {points}  ▲ Triangles: {triangles}  ";
        }
    }
}
