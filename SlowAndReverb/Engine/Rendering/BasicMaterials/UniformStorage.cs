using System.Reflection;

namespace SlowAndReverb
{
    public readonly record struct UniformStorage(string Name, PropertyInfo Property);
}
