using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using KeyboardKey = Carpet.Key;

namespace Carpet
{
    public abstract class Argument
    {
        private const char OpenSquareBracket = '[';
        private const char CloseSquareBracket = ']';

        private const char OpenAngleBracket = '<';
        private const char CloseAngleBracket = '>';

        private static readonly Dictionary<Type, Type> s_types = [];

        private static readonly StringBuilder s_builder = new StringBuilder();

        public Argument(string name, bool optional = false)
        {
            Name = name;
            Optional = optional;
        }

        public string Name { get; init; }
        public bool Optional { get; init; }

        public string TypeRepresentation { get; protected init; }

        public abstract ParsingResult Parse(string input);

        public virtual string AutoComplete(string input)
        {
            return input;
        }

        public override string ToString()
        {
            s_builder.Clear();

            if (Optional)
            {
                s_builder.Append(OpenAngleBracket);
                s_builder.Append(Name);
                s_builder.Append(CloseAngleBracket);
            }
            else
            {
                s_builder.Append(OpenSquareBracket);
                s_builder.Append(Name);
                s_builder.Append(CloseSquareBracket);
            }

            return s_builder.ToString();
        }


        public static void AddType<T>() where T : Argument
        {
            Type argumentType = typeof(T);

            AddType(argumentType);
        }

        internal static void Initialize()
        {
            Type baseType = typeof(Argument);
            Type[] types = baseType.GetNestedTypes();

            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(baseType) || type.IsGenericType)
                        continue;

                AddType(type);
            }
        }

        internal static Type GetArgumentType(Type type)
        {
            if (s_types.TryGetValue(type, out var result))
                return result;

            return null;
        }

        protected bool StartsWith(ReadOnlySpan<char> keyword, ReadOnlySpan<char> input)
        {
            return keyword.Length > 1 && keyword.StartsWith(input);
        }

        protected ParsingResult CreateConversionError(string input)
        {
            return ParsingResult.CreateConversionError(input, TypeRepresentation);
        } 

        private static void AddType(Type argumentType)
        {
            ArgumentTypeAttribute attribute =  
                argumentType.GetCustomAttribute<ArgumentTypeAttribute>();

            if (attribute is null)
            {
                DebugConsole.Log($"Argument type \"{argumentType.Name}\" does not have an" +
                    $"{typeof(ArgumentTypeAttribute).Name} attribute");

                return;
            }

            Type type = attribute.Value;

            if (s_types.ContainsKey(type))
            {
                DebugConsole.Log($"Argument type for type \"{type.Name}\" already exists");

                return;
            }

            s_types[type] = argumentType;
        }

        [ArgumentType(typeof(string))]
        public class String : Argument
        {
            public String(string name, bool optional = false) : base(name, optional)
            {
                TypeRepresentation = "string";
            }

            public override ParsingResult Parse(string input)
            {
                return ParsingResult.CreateValue(input);
            }
        }

        [ArgumentType(typeof(int))]
        public class Int : Argument
        {
            public Int(string name, bool optional = false) : base(name, optional)
            {
                TypeRepresentation = "integer";
            }

            public override ParsingResult Parse(string input)
            {
                if (int.TryParse(input, out int value))
                    return ParsingResult.CreateValue(value);

                return CreateConversionError(input);
            }
        }
        
        [ArgumentType(typeof(float))]
        public class Float : Argument
        {
            public Float(string name, bool optional = false) : base(name, optional)
            {
                TypeRepresentation = "float";
            }

            public override ParsingResult Parse(string input)
            {
                if (float.TryParse(input, out float value))
                    return ParsingResult.CreateValue(value);

                return CreateConversionError(input);
            }
        }

        [ArgumentType(typeof(bool))] 
        public class Bool : Argument
        {
            private static readonly Dictionary<string, bool> s_keywords = new Dictionary<string, bool>()
            {
                ["true"] = true,
                ["t"] = true,
                ["1"] = true,
                ["false"] = false,
                ["f"] = false,
                ["0"] = false
            };

            public Bool(string name, bool optional = false) : base(name, optional)
            {
                TypeRepresentation = "boolean";
            }

            public override ParsingResult Parse(string input)
            {
                if (s_keywords.TryGetValue(input, out bool value))
                    return ParsingResult.CreateValue(value);

                return CreateConversionError(input);
            }

            public override string AutoComplete(string input)
            {
                Span<char> lower = stackalloc char[input.Length];
                MemoryExtensions.ToLowerInvariant(input, lower);

                foreach (string keyword in s_keywords.Keys)
                    if (StartsWith(keyword, lower))
                        return keyword;

                return input;
            }
        }

        public class Enum<T> : Argument where T : struct
        {
            private static readonly Dictionary<Type, string[]> s_names = new Dictionary<Type, string[]>();

            private readonly Type _type;

            public Enum(string name, bool optional = false) : base(name, optional)
            {
                _type = typeof(T);
                TypeRepresentation = _type.Name.ToLower();

                if (!s_names.ContainsKey(_type))
                {
                    string[] names = Enum.GetNames(_type);

                    for (int i = 0; i < names.Length; i++)
                        names[i] = names[i].ToLowerInvariant();

                    s_names[_type] = names;
                }
            }

            public override ParsingResult Parse(string input)
            {
                if (Enum.TryParse(input, true, out T value))
                    return ParsingResult.CreateValue(value);

                return CreateConversionError(input);
            }

            public override string AutoComplete(string input)
            {
                Span<char> lower = stackalloc char[input.Length];
                MemoryExtensions.ToLowerInvariant(input, lower);
                
                string[] names = s_names[_type];

                foreach (string name in names)
                    if (StartsWith(name, lower))
                        return name;

                return input;
            }
        }

        [ArgumentType(typeof(KeyboardKey))]
        public class Key : Enum<KeyboardKey>
        {
            public Key(string name, bool optional = false) : base(name, optional)
            {

            }
        }
    }
}
