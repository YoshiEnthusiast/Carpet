namespace SlowAndReverb
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class UniformAttribute : Attribute
    {
        private readonly string _name;

        public UniformAttribute(string name)
        {
            _name = name;
        }

        public string Name => _name;
    }
}
