using andywiecko.ECS.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components
{
    [CustomEditor(typeof(EdgeMesh))]
    public class EdgeMeshEditor : EntityEditor
    {
        private Label label;
        private EdgeMesh edgeMesh;

        private void OnEnable()
        {
            label = CreateLabel();
            UpdateLabel();
            edgeMesh = target as EdgeMesh;
            edgeMesh.OnSerializedDataChange += UpdateLabel;
        }

        private void OnDisable()
        {
            edgeMesh.OnSerializedDataChange -= UpdateLabel;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();

            var serializedData = serializedObject.FindProperty($"<{nameof(EdgeMesh.SerializedData)}>k__BackingField");
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
            var data = (target as EdgeMesh).SerializedData;
            var points = data == null ? "???" : data.Positions.Length.ToString();
            var triangles = data == null ? "???" : (data.Edges.Length / 2).ToString();

            label.text = $" Data:  ● Points: {points}  | Edges: {triangles}  ";
        }
    }
}