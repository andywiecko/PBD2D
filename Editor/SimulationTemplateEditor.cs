using UnityEngine;

namespace andywiecko.PBD2D.Editor
{
    public class SimulationTemplateEditor : UnityEditor.Editor
    {
        [SerializeField]
        private GameObject simulationTemplate = default;

        public void Spawn()
        {
            var template = Instantiate(simulationTemplate);
            template.name = "Simulation Template";
        }
    }
}