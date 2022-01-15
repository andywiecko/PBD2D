using System;

namespace andywiecko.PBD2D.Core
{
    public class Ref<T> : IDisposable where T : struct, IDisposable
    {
        public T Value;
        public Ref(T obj) => Value = obj;
        public void Dispose() => Value.Dispose();
        public static implicit operator Ref<T>(T obj) => new(obj);
        public static implicit operator T(Ref<T> obj) => obj.Value;
    }
}
