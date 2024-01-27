using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    internal abstract class Command
    {
        public Command(string name)
        {
            Name = name;
        } 

        public ImmutableArray<Argument> Arguments { get; protected init; }
        public string Error { get; protected init; }
        public string Name { get; private init; }

        protected int RequiredArgumentsCount { get; init; }

        public abstract void Run(Span<string> input, bool catchErrors);

        protected bool CheckArgumentsCount(int inputLength)
        {
            if (inputLength > Arguments.Length || inputLength < RequiredArgumentsCount)
            {
                DebugConsole.Log($"Command \"{Name}\" does not take {inputLength} arguments", LogType.Error);

                return true;
            }

            return false;
        }
    }
}
