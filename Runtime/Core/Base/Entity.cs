using andywiecko.BurstCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    [DisallowMultipleComponent]
    public abstract class Entity : MonoBehaviour
    {
        [field: SerializeField]
        public World World { get; private set; } = default;

        private readonly List<IDisposable> refsToDisposeOnDestroy = new();

        protected void DisposeOnDestroy(params IDisposable[] references)
        {
            foreach (var reference in references)
            {
                refsToDisposeOnDestroy.Add(reference);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var reference in refsToDisposeOnDestroy)
            {
                reference.Dispose();
            }
        }
    }
}