namespace Carpet
{
    public class BasicMaterial : Material
    {
        public BasicMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram(Content.DefaultShaderName);
        }
    }
}
