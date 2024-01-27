using System.Reflection;

namespace Carpet
{
    public readonly record struct UniformStorage(string Name, PropertyInfo Property);
}
