using static andywiecko.BurstTriangulator.Triangulator;

namespace andywiecko.PBD2D.Core
{
    public static class TriangulatorExtensions
    {
        public static void CopyFrom(this TriangulationSettings @this, TriangulationSettings settings)
        {
            @this.BatchCount = settings.BatchCount;
            @this.ConstrainEdges = settings.ConstrainEdges;
            @this.MaximumArea = settings.MaximumArea;
            @this.MinimumAngle = settings.MinimumAngle;
            @this.MinimumArea = settings.MinimumArea;
            @this.RefineMesh = settings.RefineMesh;
            @this.RestoreBoundary = settings.RestoreBoundary;
            @this.ValidateInput = settings.ValidateInput;
        }
    }
}