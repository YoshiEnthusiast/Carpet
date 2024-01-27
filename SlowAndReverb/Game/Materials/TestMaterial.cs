namespace Carpet
{
    public sealed class TestMaterial : Material
    {
        public TestMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("test");
        }

        [Uniform("u_Time")] public float Time { get; set; }
    }
}
