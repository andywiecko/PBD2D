using UnityEditor;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshRenderer))]
    public class TriMeshRendererEditor : UnityEditor.Editor
    {
        private TriMeshRenderer TriMeshRenderer => target as TriMeshRenderer;

        private void OnDestroy()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (TriMeshRenderer == null && TriMeshRenderer.RendererTransform != null)
            {
                DestroyImmediate(TriMeshRenderer.RendererTransform.gameObject);
            }
        }
    }
}
