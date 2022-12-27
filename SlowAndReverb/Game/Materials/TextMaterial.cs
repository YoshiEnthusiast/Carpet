namespace SlowAndReverb
{
    public class TextMaterial : Material
    {
        public TextMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("text");
        }

        [Uniform("u_OutlineColor")] public Color OutlineColor { get; set; }
        [Uniform("u_OutlineWidth")] public int OutlineWidth { get; set; }
    }
}
