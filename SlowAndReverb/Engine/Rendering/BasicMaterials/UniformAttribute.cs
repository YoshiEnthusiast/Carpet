using System;

namespace Carpet
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class UniformAttribute : Attribute
    {
        public UniformAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private init; }
    }
}
