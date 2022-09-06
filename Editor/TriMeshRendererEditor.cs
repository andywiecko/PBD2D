using andywiecko.PBD2D.Components;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    [CustomEditor(typeof(TriMeshRenderer))]
    public class TriMeshRendererEditor : UnityEditor.Editor
    {
        private TriMeshRenderer Target => target as TriMeshRenderer;
        private TriMesh triMesh;

        private void OnEnable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            triMesh = Target.GetComponent<TriMesh>();
            triMesh.OnSerializedDataChange += Target.UpdateMeshReference;
        }

        private void OnDisable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Target == null)
            {
                return;
            }

            triMesh.OnSerializedDataChange -= Target.UpdateMeshReference;
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                triMesh.OnSerializedDataChange -= Target.UpdateMeshReference;
            }
        }
    }
}
