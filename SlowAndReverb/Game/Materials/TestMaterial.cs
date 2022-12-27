namespace SlowAndReverb
{
    public sealed class TestMaterial : Material
    {
        public TestMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("test");
        }

        [Uniform("u_Time")] public float Time { get; set; }
    }
}
