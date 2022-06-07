using UnityEngine;

namespace andywiecko.PBD2D.Editor.Tests
{
    public static class TestUtils
    {
        public static void New<T>(ref T monobehaviour, string name = "") where T : MonoBehaviour
        {
            monobehaviour = new GameObject(name).AddComponent<T>();
        }
    }
}