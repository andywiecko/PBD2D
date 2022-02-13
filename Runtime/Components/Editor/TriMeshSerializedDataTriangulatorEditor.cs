using andywiecko.BurstTriangulator;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace andywiecko.PBD2D.Components.Editor
{
    [CustomEditor(typeof(TriMeshSerializedDataTriangulator))]
    public class TriMeshSerializedDataTriangulatorEditor : UnityEditor.Editor
    {
        private TriMeshSerializedDataTriangulator Data => target as TriMeshSerializedDataTriangulator;

        private Vector2 AreaValues { get => Data.AreaValues; set => Data.AreaValues = value; }
        private float AngleValue { get => Data.AngleValue; set => Data.AngleValue = value; }
        private bool RefineMesh  => Data.RefineMesh;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new IMGUIContainer(base.OnInspectorGUI));

            var areaSlider = new MinMaxSlider(minValue: AreaValues.x, maxValue: AreaValues.y, minLimit: 0, maxLimit: 2);
            areaSlider.RegisterValueChangedCallback(minmax => AreaValues = minmax.newValue);

            var angleSlider = new Slider(start: 10, end: 40);
            angleSlider.RegisterValueChangedCallback(evt => AngleValue = evt.newValue);

            var triangulateButton = new Button(() =>
            {
                using var triangulator = new Triangulator(capacity: 64 * 1024, Allocator.Persistent);
                using var data = new NativeArray<float2>(Data.colliderPoints, Allocator.TempJob);

                triangulator.Settings.MinimumArea = AreaValues.x;
                triangulator.Settings.MaximumArea = AreaValues.y;
                triangulator.Settings.MinimumAngle = math.radians(AngleValue);
                triangulator.Settings.RefineMesh = RefineMesh;
                triangulator.Schedule(data.AsReadOnly(), default).Complete();

                Data.CopyDataFromTriangulation(triangulator);
                Data.RegenerateMesh();

                EditorUtility.SetDirty(Data);
            })
            { text = "Triangulate" };

            root.RegisterCallback<ChangeEvent<SerializedPropertyChangeEvent>>(evt =>
            {
                areaSlider.value = AreaValues;
                angleSlider.value = AngleValue;
                root.MarkDirtyRepaint();
            });


            root.Add(areaSlider);
            root.Add(angleSlider);
            root.Add(triangulateButton);

            return root;
        }
    }
}