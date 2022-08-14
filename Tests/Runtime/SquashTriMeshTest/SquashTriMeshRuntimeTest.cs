using andywiecko.BurstCollections;
using andywiecko.ECS;
using andywiecko.PBD2D.Components;
using andywiecko.PBD2D.Systems;
using andywiecko.PBD2D.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace andywiecko.PBD2D.Tests
{
    public class SquashTriMeshRuntimeTest : RuntimeTest
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return LoadSceneAsync();
            yield return new WaitForSeconds(3f);

            var triMeshes = FindObjectsOfType<TriMesh>();
            var manager = FindObjectOfType<SystemsManager>();

            static void SquashTriMesh(TriMesh triMesh)
            {
                var points = triMesh.PredictedPositions.Value;
                for (int i = 0; i < points.Length; i++)
                {
                    var x = points[(Id<Point>)i];
                    x.y = 2f;
                    triMesh.Positions.Value[(Id<Point>)i] = x;
                    triMesh.PredictedPositions.Value[(Id<Point>)i] = x;
                }
            }

            foreach (var t in triMeshes)
            {
                SquashTriMesh(t);
            }
            manager.SetSystemActive<PositionBasedDynamicsStepStartSystem>(false);
            manager.SetSystemActive<PositionBasedDynamicsStepEndSystem>(false);
            yield return new WaitForSeconds(1f);

            manager.SetSystemActive<PositionBasedDynamicsStepStartSystem>(true);
            manager.SetSystemActive<PositionBasedDynamicsStepEndSystem>(true);
            yield return new WaitForSeconds(3f);
        }
    }
}