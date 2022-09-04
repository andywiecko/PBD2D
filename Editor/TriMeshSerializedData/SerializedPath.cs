using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    [Serializable]
    public class SerializedPath : IEnumerable<float2>
    {
        [field: SerializeField]
        public float2[] Data { get; private set; } = { };
        public IEnumerator<float2> GetEnumerator() => (Data as IEnumerable<float2>).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public SerializedPath(float2[] data) => Data = data;
    }
}
