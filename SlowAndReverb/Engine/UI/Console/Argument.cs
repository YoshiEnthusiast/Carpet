using System;
using System.Collections.Generic;
using GameEntity = SlowAndReverb.Entity;

namespace SlowAndReverb
{
    public abstract class Argument
    {
        protected Argument(string name, bool optional)
        {
            Name = name;
            Optional = optional;
        }

        public string Name { get; private init; }
        public bool Optional { get; private init; }

        public abstract ParsingResult Parse(string line);

        public virtual string AutoComplete(string line)
        {
            return line;
        }

        private class String : Argument
        {
            protected String(string name, bool optional) : base(name, optional)
            {

            }

            public override ParsingResult Parse(string line)
            {
                return ParsingResult.CreateValue(line);
            }
        }

        private class Integer : Argument
        {
            protected Integer(string name, bool optional) : base(name, optional)
            {

            }

            public override ParsingResult Parse(string line)
            {
                if (int.TryParse(line, out int value))
                    return ParsingResult.CreateValue(value);

                return ParsingResult.CreateError($"""Could not convert "{line}" to integer" """);
            }
        }

        private class Float : Argument
        {
            protected Float(string name, bool optional) : base(name, optional)
            {

            }

            public override ParsingResult Parse(string line)
            {
                if (float.TryParse(line, out float value))
                    return ParsingResult.CreateValue(value);

                return ParsingResult.CreateConvertionError(line, "integer");
            }
        }

        private class Bool : Argument
        {
            private static readonly Dictionary<string, bool> s_binaryStateWords = new Dictionary<string, bool>()
            {
                ["true"] = true,
                ["1"] = true,
                ["false"] = false,
                ["0"] = false
            };

            protected Bool(string name, bool optional) : base(name, optional)
            {

            }

            public override ParsingResult Parse(string line)
            {
                if (s_binaryStateWords.TryGetValue(line, out bool value))
                    return ParsingResult.CreateValue(value);

                return ParsingResult.CreateConvertionError(line, "bool");
            }

            public override string AutoComplete(string line)
            {
                foreach (string word in s_binaryStateWords.Keys)
                    if (word.StartsWith(line))
                        return word;

                return line;
            }
        }

        private class Entity : Argument
        {
            protected Entity(string name, bool optional) : base(name, optional)
            {

            }

            public override ParsingResult Parse(string line)
            {
                Type type = Editor.GetEntityTypeByName(line);

                if (type is null)
                    return ParsingResult.CreateError($"""Entity {line} does not exist""");

                GameEntity entity = Editor.CreateEntityOfType(type);

                return ParsingResult.CreateValue(entity);
            }

            public override string AutoComplete(string line)
            {
                foreach (string name in Editor.EntityNamesToLower)
                    if (name.StartsWith(line))
                        return name;

                return line;
            }
        }
    }
}
