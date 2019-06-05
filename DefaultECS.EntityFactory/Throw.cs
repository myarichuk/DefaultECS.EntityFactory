using System;
using System.Runtime.CompilerServices;
using Fasterflect;

namespace DefaultECS.EntityFactory
{
    public static class Throw
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Exception<T>(params object[] @params) where T : Exception => 
            throw ((Exception)typeof(T).CreateInstance(@params));
    }
}
