using UnityEngine;

namespace andywiecko.PBD2D.Core
{
    [DisallowMultipleComponent]
    public class SystemsManager : MonoBehaviour
    {
        [field: SerializeField]
        public World World { get; private set; } = default;
    }
}