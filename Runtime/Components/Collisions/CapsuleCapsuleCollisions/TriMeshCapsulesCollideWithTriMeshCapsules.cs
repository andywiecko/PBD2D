using andywiecko.PBD2D.Core;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PBD2D:TriMesh.Components/Collisions/Collide With TriMesh (Capsules)")]
    public class TriMeshCapsulesCollideWithTriMeshCapsules : TriMeshCapsulesCollideWith, ITriMeshCapsulesCollideWithTriMeshCapsules { }
}