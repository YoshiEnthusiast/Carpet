using System;
using System.Collections.Immutable;

namespace Carpet
{
    internal sealed class ActionCommand : Command
    {
        private readonly Action<Arguments> _action;

        public ActionCommand(string name, Argument[] arguments, Action<Arguments> action) : base(name)
        {
            if (arguments is null)
                arguments = Array.Empty<Argument>();    

            _action = action;
            Arguments = ImmutableArray.Create(arguments);

            int argumentsCount = Arguments.Length;
            RequiredArgumentsCount = argumentsCount;
            bool requiredArgumentMet = false;

            for (int i = argumentsCount - 1; i >= 0; i--)
            {
                if (Arguments[i].Optional)
                {
                    if (requiredArgumentMet)
                    {
                        Error = $"Command \"{Name}\": optional arguments must appear before " +
                            $"all required arguments";

                        return;
                    }

                    RequiredArgumentsCount--;

                    continue;
                }

                requiredArgumentMet = true;
            }
        }

        public override void Run(Span<string> input, bool catchErrors)
        {
            int inputLength = input.Length;
            int argumentsCount = Arguments.Length;

            if (CheckArgumentsCount(inputLength))
                return;

            var info = new ArgumentInfo[argumentsCount];

            for (int i = 0; i < argumentsCount; i++)
            {
                Argument argument = Arguments[i];
                string name = argument.Name;

                if (i < inputLength)
                {
                    string givenArgument = input[i];
                    ParsingResult result = argument.Parse(givenArgument);

                    if (result.HasError)
                    {
                        DebugConsole.Log($"Error parsing argument \"{name}\" ({i}): " +
                            $"{result.Error}", LogType.Error);

                        return;
                    }

                    info[i] = ArgumentInfo.CreateValue(name, result.Value);
                }
                else
                {
                    info[i] = ArgumentInfo.CreateOmitted(name);
                }
            }

            var arguments = new Arguments(info);

            try
            {
                _action.Invoke(arguments);
            }
            catch (Exception exception) 
            {
                if (catchErrors)
                {
                    DebugConsole.Log(exception.Message, LogType.Error);

                    return;
                }

                throw;
            }
        }
    }
}
