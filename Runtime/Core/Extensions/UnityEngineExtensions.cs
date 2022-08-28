using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class UnityEngineExtensions
    {
        public static T TryAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var component))
            {
                return component;
            }
            return gameObject.AddComponent<T>();
        }

        public static T TryAddComponent<T>(this Transform transform) where T : Component => transform.gameObject.TryAddComponent<T>();
    }
}