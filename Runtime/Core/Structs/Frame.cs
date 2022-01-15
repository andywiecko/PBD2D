using andywiecko.BurstCollections;

namespace andywiecko.PBD2D.Core
{
    public readonly struct Frame
    {
        public readonly Id<Point> IdA;
        public readonly Id<Point> IdB;
        public readonly Id<Point> IdC;

        public Frame(Id<Point> idA, Id<Point> idB, Id<Point> idC)
        {
            IdA = idA;
            IdB = idB;
            IdC = idC;
        }

        public static explicit operator Frame((int idA, int idB, int idC) ids) => new Frame(ids);

        private Frame((int idA, int idB, int idC) ids) : this((Id<Point>)ids.idA, (Id<Point>)ids.idB, (Id<Point>)ids.idC) { }

        public void Deconstruct(out Id<Point> idA, out Id<Point> idB, out Id<Point> idC)
        {
            idA = IdA;
            idB = IdB;
            idC = IdC;
        }
    }
}
