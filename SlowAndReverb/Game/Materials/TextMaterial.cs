namespace Carpet
{
    public class TextMaterial : Material
    {
        public TextMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("text");
        }

        [Uniform("u_OutlineColor")] public Color OutlineColor { get; set; }
        [Uniform("u_OutlineWidth")] public int OutlineWidth { get; set; }
    }
}
