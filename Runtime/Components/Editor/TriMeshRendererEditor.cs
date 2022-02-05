using andywiecko.PBD2D.Core;
using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshRenderer))]
    public class TriMeshRendererEditor : UnityEditor.Editor
    {
        private TriMeshRenderer Target => target as TriMeshRenderer;
        private TriMesh triMesh;

        private void TryInitializeRenderer()
        {
            EditorApplication.delayCall -= TryInitializeRenderer;

            var newTransform = TryCreateRendererTransform();
            if (newTransform == null)
            {
                return;
            }

            newTransform.TryAddComponent<MeshRenderer>();
            var filter = newTransform.TryAddComponent<MeshFilter>();
            var data = triMesh.SerializedData;
            var mesh = data == null ? default : data.Mesh;
            if (mesh)
            {
                filter.sharedMesh = Instantiate(mesh);
            }

            Target.RendererTransform = newTransform;
            EditorUtility.SetDirty(Target);
        }

        private Transform TryCreateRendererTransform()
        {
            var name = "RendererTransform";
            if (Target == null)
            {
                return default;
            }
            var rendererTransform = Target.transform.Find(name);

            if (rendererTransform == null)
            {
                rendererTransform = new GameObject(name).transform;
                rendererTransform.SetParent(Target.transform);
                rendererTransform.localPosition = float3.zero;
            }
            return rendererTransform;
        }

        private void OnEnable()
        {
            triMesh = Target.GetComponent<TriMesh>();
            triMesh.OnSerializedDataChange += TryInitializeRenderer;

            if (Target.RendererTransform == null || Target.transform.Find("RendererTransform") == null)
            {
                CallOnce(TryInitializeRenderer);
            }
        }

        private void CallOnce(Action a)
        {
            EditorApplication.delayCall += Delayed;
            void Delayed()
            {
                EditorApplication.delayCall -= Delayed;
                a();
            }
        }

        private void OnDestroy()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (Target == null && Target.RendererTransform != null)
            {
                triMesh.OnSerializedDataChange -= TryInitializeRenderer;
            }
        }
    }
}
