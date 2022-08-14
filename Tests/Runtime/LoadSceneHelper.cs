using andywiecko.PBD2D.Tests;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(LoadSceneHelperTestRunnerCallback))]

namespace andywiecko.PBD2D.Tests
{
    public class LoadSceneHelper : ScriptableSingleton<LoadSceneHelper>
    {
        [SerializeField]
        private List<string> keys = new();

        [SerializeField]
        private List<SceneAsset> values = new();

        public SceneAsset GetSceneAsset(RuntimeTest test)
        {
            return values[keys.IndexOf(test.GetType().AssemblyQualifiedName)];
        }

        public void Validate()
        {
            keys.Clear();
            values.Clear();
        }

        public void CacheSceneAsset(RuntimeTest test, SceneAsset scene)
        {
            var type = test.GetType().AssemblyQualifiedName;
            var id = keys.IndexOf(type);
            if (id == -1)
            {
                keys.Add(type);
                values.Add(scene);
            }
            else
            {
                values[id] = scene;
            }

            Save(true);
        }
    }

    public class LoadSceneHelperTestRunnerCallback : ITestRunCallback
    {
        public void RunStarted(ITest testsToRun)
        {
            Debug.Log("Run Started");
        }

        public void RunFinished(ITestResult testResults)
        {
            Debug.Log("Run Finished");
        }

        public void TestStarted(ITest test) { }

        public void TestFinished(ITestResult result) { }
    }
}