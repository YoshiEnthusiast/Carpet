namespace Carpet
{
    public class SolidColorMaterial : Material
    {
        public SolidColorMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("solidColor");
        }
    }
}
