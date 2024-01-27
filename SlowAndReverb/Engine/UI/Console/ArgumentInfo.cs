namespace Carpet
{ 
    public struct ArgumentInfo
    {
        private ArgumentInfo(string name, object value)
        {
            Name = name;
            Value = value;
        }

        private ArgumentInfo(string name)
        {
            Name = name;
            Omitted = true;
        }

        public string Name { get; private init; }
        public object Value { get; private init; }
        public bool Omitted { get; private init; }

        public static ArgumentInfo CreateValue(string name, object value)
        {
            return new ArgumentInfo(name, value);
        }

        public static ArgumentInfo CreateOmitted(string name)
        {
            return new ArgumentInfo(name);
        }
    }
}
