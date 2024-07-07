namespace Carpet
{
    public class ForegroundMaterial : Material
    {
        public ForegroundMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("foreground");
        }

        [Uniform("u_Darkness")] public float Darkness { get; set; } = 0.1f;
    }
}
