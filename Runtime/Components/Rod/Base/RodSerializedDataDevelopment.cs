using System;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [CreateAssetMenu(
        fileName = "RodSerializedDataDevelopment",
        menuName = "PBD2D/Rod/Serialized Data (Development)"
    )]
    [Obsolete("Don't use! This is for development purposes only!")]
    public class RodSerializedDataDevelopment : RodSerializedData
    {
        private void Awake()
        {
            Positions = new float2[]
            {
                new(0, 0),
                new(1, 0),
                new(2, 0),
                new(3, 0),
                new(4, 0),
                new(5, 0),
                new(6, 0),
                new(7, 0),
                new(8, 0),
                new(9, 0),
            };

            Edges = new int[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 4,
                4, 5,
                5, 6,
                6, 7,
                7, 8,
                8, 9,
            };
        }
    }
}