namespace Carpet
{
    public class RGBTextMaterial : TextMaterial
    {
        public RGBTextMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("rgbText");
        }

        [Uniform("u_Time")] public float Time { get; set; }
    }
}
