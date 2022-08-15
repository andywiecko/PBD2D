using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [RequireComponent(typeof(TriMesh))]
    [DisallowMultipleComponent]
    [Category(PBDCategory.Collisions)]
    public class TriMeshCapsulesCollideWithTriMeshCapsules : TriMeshCapsulesCollideWith, ITriMeshCapsulesCollideWithTriMeshCapsules { }
}