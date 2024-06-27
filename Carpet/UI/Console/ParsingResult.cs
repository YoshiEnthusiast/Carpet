namespace Carpet
{
    public struct ParsingResult
    {
        public object Value { get; private init; }
        public string Error { get; private init; }

        public bool HasError => Error is not null;

        public static ParsingResult CreateValue(object value)
        {
            var result = new ParsingResult()
            {
                Value = value
            };

            return result;
        }

        public static ParsingResult CreateError(string error)
        {
            var result = new ParsingResult()
            {
                Error = error
            };

            return result;
        }

        public static ParsingResult CreateConversionError(string input, string type)
        {
            var error = $"Could not convert \"{input}\" to type {type}";

            return CreateError(error);
        }
    }
}
