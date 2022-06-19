using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Systems
{
    [Category(PBDCategory.Debug)]
    public class MouseInteractionSystem : BaseSystem<IMouseInteractionComponent>
    {
        private bool grabBody = false;
        private bool releaseBody = false;
        private float2 mousePosition;

        // TODO: this is no longer working since removing MonoBehaviour dependency for systems!
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.DrawSphere(mousePosition.ToFloat3(), 0.1f);

            foreach (var a in References)
            {
                Gizmos.DrawRay(mousePosition.ToFloat3(), a.Offset.Value.Value.ToFloat3());
            }
        }

        [SolverAction]
        public void MouseInteractionUpdate()
        {
            grabBody = false;
            releaseBody = false;

            if (Camera.main)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    grabBody = true;
                }
                if (Input.GetMouseButton(0))
                {
                    mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition).origin.ToFloat2();
                }
                if (Input.GetMouseButtonUp(0))
                {
                    releaseBody = true;
                }
            }
        }

        [BurstCompile]
        private struct GrabBodyJob : IJob
        {
            private NativeReference<Id<Point>> interactingPointId;
            private NativeReference<float2> offset;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private readonly float2 mousePosition;
            private readonly float minimalDistanceSq;

            public GrabBodyJob(IMouseInteractionComponent component, float2 mousePosition, float minimalDistance)
            {
                interactingPointId = component.InteractingPointId;
                offset = component.Offset;
                positions = component.PredictedPositions;
                this.mousePosition = mousePosition;
                this.minimalDistanceSq = minimalDistance * minimalDistance;
            }

            public void Execute()
            {
                var minDistanceSq = float.MaxValue;
                var minPointId = Id<Point>.Invalid;
                foreach (var pointId in positions.Ids)
                {
                    var position = positions[pointId];
                    var distanceSq = math.distancesq(position, mousePosition);
                    if (minDistanceSq > distanceSq)
                    {
                        minDistanceSq = distanceSq;
                        minPointId = pointId;
                    }
                }

                if (minDistanceSq > minimalDistanceSq)
                {
                    return;
                }

                interactingPointId.Value = minPointId;
                offset.Value = mousePosition - positions[minPointId];
            }
        }

        [BurstCompile]
        private struct DragBody : IJob
        {
            private NativeReference<Id<Point>> interactingPointId;
            private NativeReference<float2> offset;
            private NativeIndexedArray<Id<Point>, float2> positions;
            private readonly float2 mousePosition;

            public DragBody(IMouseInteractionComponent component, float2 mousePosition)
            {
                interactingPointId = component.InteractingPointId;
                offset = component.Offset;
                positions = component.PredictedPositions;
                this.mousePosition = mousePosition;
            }

            public void Execute()
            {
                var pointId = interactingPointId.Value;
                if (!pointId.IsValid)
                {
                    return;
                }

                positions[pointId] = mousePosition + offset.Value;
            }
        }

        public override JobHandle Schedule(JobHandle dependencies)
        {
            foreach (var component in References)
            {
                if (grabBody)
                {
                    dependencies = new GrabBodyJob(component, mousePosition, 1f).Schedule(dependencies);
                }

                dependencies = new DragBody(component, mousePosition).Schedule(dependencies);

                if (releaseBody)
                {
                    dependencies = new CommonJobs.SetNativeReferenceValueJob<Id<Point>>(component.InteractingPointId, Id<Point>.Invalid).Schedule(dependencies);
                }
            }
            return dependencies;
        }
    }
}
