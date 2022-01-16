using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public static class ComponentIdCounter
    {
        private static int count = 0;

        public static Id<IComponent> GetNext() => (Id<IComponent>)count++;
    }
}