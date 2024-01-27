using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Carpet
{
    internal sealed class MethodCommand : Command
    {
        private readonly MethodInfo _method;
        private readonly ParameterInfo[] _parameters;

        public MethodCommand(string name, MethodInfo method) : base(name)
        {
            _method = method;

            if (!_method.IsStatic)
            {
                Error = "Command method must be static";

                return;
            }

            _parameters = method.GetParameters();

            int parametersCount = _parameters.Length;
            var arguments = new Argument[parametersCount];

            for (int i = 0; i < parametersCount; i++)
            {
                ParameterInfo parameter = _parameters[i];

                Type type = parameter.ParameterType;
                bool optional = parameter.HasDefaultValue;
                string parameterName = parameter.Name.ToLowerInvariant();
                Type argumentType = Argument.GetArgumentType(type);

                if (argumentType is null)
                {
                    Error = $"Argument type for type \"{type.Name}\" not found";

                    return;
                }

                if (!optional)
                    RequiredArgumentsCount++;

                Argument argument = (Argument)Activator.CreateInstance(argumentType, new object[]
                {
                    parameterName,
                    optional
                });

                arguments[i] = argument;
            }

            Arguments = arguments.ToImmutableArray();
        }

        public override void Run(Span<string> input, bool catchErrors)
        {
            int inputLength = input.Length;
            int argumentsCount = Arguments.Length;

            if (CheckArgumentsCount(inputLength))
                return;

            var parameters = new object[argumentsCount];

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

                    parameters[i] = result.Value;
                }
                else
                {
                    ParameterInfo parameter = _parameters[i];

                    parameters[i] = parameter.DefaultValue;
                }
            }

            try
            {
                _method.Invoke(null, parameters);
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
