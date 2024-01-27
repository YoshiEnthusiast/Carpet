using System;

namespace Carpet
{
    public sealed class ArgumentTypeAttribute : Attribute
    {
        public ArgumentTypeAttribute(Type value)
        {
            Value = value;
        }

        public Type Value { get; private init; }
    }
}
