﻿using System.Reflection;

namespace Carpet.Examples.Platforming
{
    [ArgumentType(typeof(Entity))]
    public sealed class EntityArgument : Argument
    {
        private static readonly Dictionary<string, Type> s_entityTypes = [];

        public EntityArgument(string name, bool optional = false) : base(name, optional)
        {
            TypeRepresentation = "entity";
        }

        static EntityArgument()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Entity)))
                    continue;

                if (type.IsGenericType || type.IsAbstract)
                    continue;

                string name = type.Name
                    .ToLowerInvariant();

                s_entityTypes[name] = type;
            }

            s_entityTypes["entity"] = typeof(Carpet.Entity);
        }

        public override ParsingResult Parse(string input)
        {
            string lower = input.ToLowerInvariant();

            if (s_entityTypes.TryGetValue(lower, out Type type))
            {
                object entity = Activator.CreateInstance(type, new object[]
                {
                    0f, 
                    0f
                });

                return ParsingResult.CreateValue(entity);
            }

            return ParsingResult.CreateError($"Entity \"{input}\" does not exist");
        }

        public override string AutoComplete(string input)
        {
            Span<char> lower = stackalloc char[input.Length];
            MemoryExtensions.ToLowerInvariant(input, lower);

            foreach (string name in s_entityTypes.Keys)
                if (StartsWith(name, lower))
                    return name;

            return input;
        }
    }
}
