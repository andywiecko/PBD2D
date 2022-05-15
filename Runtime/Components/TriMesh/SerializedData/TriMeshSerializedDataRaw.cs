using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [CreateAssetMenu(
        fileName = "TriMeshSerializedData (Raw)",
        menuName = "PBD2D/TriMesh/TriMeshSerializedData (Raw)")]
    public class TriMeshSerializedDataRaw : TriMeshSerializedData
    {
        [SerializeField]
        private List<Vector2> uvs = new();

        protected override void UpdateUVs()
        {
            UVs = new Vector2[Positions.Length];
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // TODO: add event to handle this
            if (uvs.Count < UVs.Length)
            {
                while (uvs.Count < UVs.Length) uvs.Add(default);
            }

            if (uvs.Count > UVs.Length)
            {
                while (uvs.Count > UVs.Length) uvs.RemoveAt(uvs.Count - 1);
            }

            UVs = uvs.ToArray();
        }
    }
}