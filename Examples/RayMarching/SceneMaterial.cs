namespace Carpet.Examples.RayMarching
{
    public class SceneMaterial : Material
    {
        public SceneMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("scene");
        }
    }
}
