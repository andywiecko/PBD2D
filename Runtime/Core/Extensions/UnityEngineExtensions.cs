using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    public static class UnityEngineExtensions
    {
        public static T TryAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        public static T TryAddComponent<T>(this Transform transform) where T : Component => transform.gameObject.TryAddComponent<T>();
    }
}