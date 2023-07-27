using System;

namespace SlowAndReverb
{
    public sealed class Command
    {
        private readonly Argument[] _arguments;
        private readonly Delegate _callback;

        public Command(string name, Delegate callback, Argument[] arguments)
        {
            _arguments = arguments;
            _callback = callback;

            Name = name;
        }

        public string Name { get; private init; }

        public void Run(string[] input)
        {

        }
    }
}
