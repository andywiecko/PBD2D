using andywiecko.PBD2D.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Editor
{
    [CustomEditor(typeof(EdgeMeshRenderer))]
    public class EdgeMeshRendererEditor : UnityEditor.Editor
    {
        private EdgeMeshRenderer Target => target as EdgeMeshRenderer;
        private EdgeMesh edgeMesh;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imgui = new IMGUIContainer(base.OnInspectorGUI) { name = "imgui" };
            root.Add(imgui);

            var prefabProperty = serializedObject.FindProperty("rendererPrefab");
            var prefabField = new PropertyField(prefabProperty);
            prefabField.RegisterValueChangeCallback(_ => Target.UpdateLineRenderersReferences());
            prefabField.SetEnabled(!Application.isPlaying);
            root.Add(prefabField);

            return root;
        }

        private void OnEnable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            edgeMesh = Target.GetComponent<EdgeMesh>();
            edgeMesh.OnSerializedDataChange += Target.UpdateLineRenderersReferences;
        }

        private void OnDisable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Target == null)
            {
                return;
            }

            edgeMesh.OnSerializedDataChange -= Target.UpdateLineRenderersReferences;
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying && Target != null)
            {
                edgeMesh.OnSerializedDataChange -= Target.UpdateLineRenderersReferences;
            }
        }
    }
}