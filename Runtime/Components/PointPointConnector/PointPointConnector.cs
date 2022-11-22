using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    public class PointPointConnector : Entity
    {
        public Ref<NativeList<PointPointConnectorConstraint>> Constraints { get; private set; }

        public IPointsProvider Connectee { get; private set; }
        [SerializeField, Tooltip("All entities which implement `" + nameof(IPointsProvider) + "` can be set as target.")]
        private Entity connectee = default;

        public IPointsProvider Connecter { get; private set; }
        [SerializeField, Tooltip("All entities which implement `" + nameof(IPointsProvider) + "` can be set as target.")]
        private Entity connecter = default;

        [field: SerializeField, Range(0, 1)]
        public float Stiffness { get; private set; } = 1;

        [field: SerializeField, Min(0)]
        public float Compliance { get; private set; } = 0;

        [field: SerializeField, Range(0, 1)]
        public float Weight { get; private set; } = 0.5f;

        protected override void Awake()
        {
            Connectee = connectee as IPointsProvider;
            Connecter = connecter as IPointsProvider;

            DisposeOnDestroy(
                Constraints = new NativeList<PointPointConnectorConstraint>(Allocator.Persistent)
            );
        }

        private void OnValidate()
        {
            PointsProviderUtils.Validate(ref connectee, this);
            PointsProviderUtils.Validate(ref connecter, this);

            if (connectee is { } && connectee == connecter)
            {
                connectee = connecter = default;
                Debug.LogError($"[{name}]: {nameof(PointPointConnector)} for same entity is not supported!", this);
            }
        }
    }
}