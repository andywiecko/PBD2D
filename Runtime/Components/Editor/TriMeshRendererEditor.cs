using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshRenderer))]
    public class TriMeshRendererEditor : UnityEditor.Editor
    {
        private TriMeshRenderer Target => target as TriMeshRenderer;
        private TriMesh triMesh;

        private void OnEnable()
        {
            triMesh = Target.GetComponent<TriMesh>();
            triMesh.OnSerializedDataChange += Target.UpdateMeshReference;
        }

        private void OnDestroy()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (Target == null && Target.RendererTransform != null)
            {
                triMesh.OnSerializedDataChange -= Target.UpdateMeshReference;
            }
        }
    }
}
