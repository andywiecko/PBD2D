using andywiecko.ECS;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Components.Editor
{
    public static class EntitiesEditor
    {
        // TODO: add source generator for these

        private const string path = "GameObject/PBD2D/";

        [MenuItem(path + nameof(TriMesh))]
        public static void CreateTriMesh() => Create<TriMesh>();

        [MenuItem(path + nameof(Ground))]
        public static void CreateGround() => Create<Ground>();

        private static void Create<T>() where T : Entity
        {
            Selection.activeObject = new GameObject(typeof(T).Name).AddComponent<T>();
            EditorApplication.delayCall += subscribe;

            static void subscribe()
            {
                EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
                EditorApplication.delayCall -= subscribe;
            }
        }
    }
}