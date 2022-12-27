namespace SlowAndReverb
{
    public class RGBTextMaterial : TextMaterial
    {
        public RGBTextMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("rgbText");
        }

        [Uniform("u_Time")] public float Time { get; set; }
    }
}
