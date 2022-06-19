using andywiecko.ECS.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMesh))]
    public class TriMeshEditor : EntityEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();
            root.Insert(1, CreateLabel());
            return root;
        }

        private VisualElement CreateLabel()
        {
            var label = new Label()
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

            var data = (target as TriMesh).SerializedData;
            var points = data == null ? "???" : data.Positions.Length.ToString();
            var edges = data == null ? "???" : (data.Edges.Length / 2).ToString();
            var triangles = data == null ? "???" : (data.Triangles.Length / 3).ToString();

            label.text = $" Data:  ● Points: {points}    ▬ Edges: {edges}    ▲ Triangles: {triangles}  "; ;

            return label;
        }
    }
}
