using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshRenderer))]
    public class TriMeshRendererEditor : UnityEditor.Editor
    {
        private TriMeshRenderer TriMeshRenderer => target as TriMeshRenderer;

        private bool duringEditor = false;

        private void OnEnable() => duringEditor = Application.isEditor && !Application.isPlaying;

        private void OnDestroy()
        {
            if (duringEditor && TriMeshRenderer == null)
            {
                if (TriMeshRenderer.RendererTransform != null)
                {
                    DestroyImmediate(TriMeshRenderer.RendererTransform.gameObject);
                }
            }
        }
    }
}
