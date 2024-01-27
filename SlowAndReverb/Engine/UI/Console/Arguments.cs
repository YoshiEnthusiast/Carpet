using System;
using System.Collections.Generic;

namespace Carpet
{
    public sealed class Arguments
    {
        private readonly Dictionary<string, Argument> _arguments = new Dictionary<string, Argument>();

        public Arguments(Span<ArgumentInfo> argumentInfo)
        {
            foreach (ArgumentInfo info in argumentInfo)
            {
                var argument = new Argument(info.Value, info.Omitted);

                _arguments[info.Name] = argument;
            }
        }

        public T Get<T>(string name, T defaultValue)
        {
            Argument argument = GetArgument(name);

            if (argument.Omitted)
                return defaultValue;

            return (T)argument.Value;
        }

        public bool TryGet<T>(string name, out T result)
        {
            Argument argument = GetArgument(name);

            if (argument.Omitted) 
            {
                result = default;
                return false;
            }

            result = (T)argument.Value;
            return true;
        }

        public T Get<T>(string name)
        {
            return Get<T>(name, default);
        }

        public bool Given(string name)
        {
            Argument argument = GetArgument(name);

            return !argument.Omitted;
        }

        private Argument GetArgument(string name)
        {
            if (!_arguments.TryGetValue(name, out Argument argument))
                throw new Exception($"Argument \"{name}\" does not exist");

            return argument;
        }

        private readonly record struct Argument(object Value, bool Omitted);
    }
}
