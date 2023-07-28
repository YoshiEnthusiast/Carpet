using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public struct ParsingResult
    {
        private ParsingResult(object value)
        {
            Value = value;
        }

        private ParsingResult(string error)
        {
            Error = error;
        }

        public object Value { get; private init; }
        public string Error { get; private init; }

        public static ParsingResult CreateValue(object value)
        {
            return new ParsingResult(value);
        }

        public static ParsingResult CreateError(string error)
        {
            return new ParsingResult(error);
        }

        public static ParsingResult CreateConvertionError(string value, string type)
        {
            return CreateError($"""Could not convert "{value}" to {type}""");
        }

        public override string ToString()
        {
            if (Error is not null)
                return Error;

            return Value.ToString();
        }
    }
}
