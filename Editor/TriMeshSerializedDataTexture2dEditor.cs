using UnityEditor;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshSerializedDataTexture2d), editorForChildClasses: true)]
    public class TriMeshSerializedDataTexture2dEditor : UnityEditor.Editor
    {
        private TriMeshSerializedDataTexture2d Target => target as TriMeshSerializedDataTexture2d;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new IMGUIContainer(base.OnInspectorGUI));

            var triangulateButton = new Button(() =>
            {
                Target.Triangulate();
                EditorUtility.SetDirty(Target);
            })
            { text = "Triangulate" };

            root.Add(triangulateButton);

            return root;
        }
    }
}