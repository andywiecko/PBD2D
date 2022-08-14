using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace andywiecko.PBD2D.Tests
{
    public abstract class RuntimeTest : Editor, IPrebuildSetup
    {
        [SerializeField]
        protected SceneAsset scene;

        public void Setup() => LoadSceneHelper.instance.CacheSceneAsset(this, scene);

        protected AsyncOperation LoadSceneAsync()
        {
            var asset = LoadSceneHelper.instance.GetSceneAsset(this);
            var path = AssetDatabase.GetAssetPath(asset);
            return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new(LoadSceneMode.Single));
        }
    }
}