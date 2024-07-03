namespace Carpet.RayMarching
{
    public class SceneMaterial : Material
    {
        public SceneMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("scene");
        }
    }
}
