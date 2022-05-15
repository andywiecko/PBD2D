using andywiecko.BurstTriangulator;
using andywiecko.PBD2D.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshSerializedData), editorForChildClasses: true)]
    public class TriMeshSerializedDataEditor : UnityEditor.Editor
    {
        private TriMeshSerializedData Target => target as TriMeshSerializedData;
        private Triangulator.TriangulationSettings Settings => Target.Settings;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new IMGUIContainer(base.OnInspectorGUI));

            var areaSlider = new MinMaxSlider(
                minValue: Settings.MinimumArea,
                maxValue: Settings.MaximumArea,
                minLimit: 0,
                maxLimit: 2
            );
            areaSlider.RegisterValueChangedCallback(minmax =>
            {
                var v = minmax.newValue;
                (Settings.MinimumArea, Settings.MaximumArea) = (v.x, v.y);
            });

            var angleSlider = new Slider(start: 10, end: 40) { value = math.degrees(Settings.MinimumAngle) };
            angleSlider.RegisterValueChangedCallback(evt => Settings.MinimumAngle = math.radians(evt.newValue));

            var triangulateButton = new Button(() =>
            {
                using var positions = new NativeArray<float2>(Target.InputData.Positions, Allocator.Persistent);
                using var holes = new NativeArray<float2>(Target.InputData.Holes, Allocator.Persistent);
                using var constraints = new NativeArray<int>(Target.InputData.Constraints, Allocator.Persistent);
                using var triangulator = new Triangulator(capacity: 64 * 1024, Allocator.Persistent)
                {
                    Input =
                    {
                        Positions = positions,
                        HoleSeeds = holes,
                        ConstraintEdges = constraints
                    }
                };
                triangulator.Settings.CopyFrom(Settings);

                triangulator.Run();

                Target.CopyDataFromTriangulation(triangulator);
                Target.RegenerateMesh();

                EditorUtility.SetDirty(Target);
            })
            { text = "Triangulate" };

            //root.RegisterCallback<ChangeEvent<SerializedPropertyChangeEvent>>(evt =>
            // HACK: 
            root.RegisterCallback<PointerMoveEvent>(evt => UpdateSliders());
            root.RegisterCallback<KeyDownEvent>(evt => UpdateSliders());
            void UpdateSliders()
            {
                areaSlider.value = new(Settings.MinimumArea, Settings.MaximumArea);
                angleSlider.value = math.degrees(Settings.MinimumAngle);
                root.MarkDirtyRepaint();
            }

            root.Add(areaSlider);
            root.Add(angleSlider);
            root.Add(triangulateButton);

            return root;
        }
    }
}