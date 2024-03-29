using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class PointsLocker : Entity
    {
        public bool TransformChanged => !RigidTransform.Equals(previousRigidTransform);
        public event Action OnTransformChanged;
        public Ref<NativeArray<Point>> Points => Target.Points;
        public Ref<NativeIndexedArray<Id<Point>, float2>> Positions => Target.Positions;
        public Ref<NativeIndexedArray<Id<Point>, float>> Weights => Target.Weights;

        private IPointsProvider Target { get; set; }
        [SerializeField, Tooltip("All entities which implement `" + nameof(IPointsProvider) + "` can be set as target.")]
        private Entity target = default;

        private RigidTransform RigidTransform => new(transform.rotation, transform.position);
        private RigidTransform previousRigidTransform;

        protected override void Awake()
        {
            base.Awake();
            Target = target as IPointsProvider;
            previousRigidTransform = RigidTransform;

            if (Target == null)
            {
                foreach (var c in GetComponents<BaseComponent>())
                {
                    c.enabled = false;
                }
                throw new NullReferenceException();
            }
        }

        private void Update()
        {
            if (TransformChanged)
            {
                OnTransformChanged?.Invoke();
            }

            previousRigidTransform = RigidTransform;
        }

        private void OnValidate() => PointsProviderUtils.Validate(ref target, this);
    }
}