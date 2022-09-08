using andywiecko.ECS;
using andywiecko.PBD2D.Components;
using UnityEditor;
using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    public static class EntitiesEditor
    {
        private const string path = "GameObject/PBD2D/";

        [MenuItem(path + nameof(TriMesh))]
        public static void CreateTriMesh() => Create<TriMesh>();

        [MenuItem(path + nameof(Ground))]
        public static void CreateGround() => Create<Ground>();

        [MenuItem(path + "Simulation Template")]
        public static void CreateSimulationTemplate() => ScriptableObject
            .CreateInstance<SimulationTemplateEditor>()
            .Spawn();

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