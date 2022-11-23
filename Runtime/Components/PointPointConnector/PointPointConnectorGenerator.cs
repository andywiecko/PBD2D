using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using UnityEngine;

namespace andywiecko.PBD2D.Components
{
    [Category(PBDCategory.Generator)]
    [DisallowMultipleComponent]
    public abstract class PointPointConnectorGenerator : BaseComponent, IPointPointConnectorConstraints
    {
        float IPointPointConnectorConstraints.Stiffness => connector.Stiffness;
        float IPointPointConnectorConstraints.Compliance => connector.Compliance;
        float IPointPointConnectorConstraints.Weight => connector.Weight;
        IPointsProvider IPointPointConnectorConstraints.Connectee => connector.Connectee;
        IPointsProvider IPointPointConnectorConstraints.Connecter => connector.Connecter;
        Ref<NativeList<PointPointConnectorConstraint>> IPointPointConnectorConstraints.Constraints => connector.Constraints;

        protected PointPointConnector connector;

        public abstract void GenerateConstraints();

        protected virtual void Start()
        {
            connector = GetComponent<PointPointConnector>();

            GenerateConstraints();
        }
    }
}