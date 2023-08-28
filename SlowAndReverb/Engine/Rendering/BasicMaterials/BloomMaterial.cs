namespace SlowAndReverb
{
    public class BloomMaterial : Material
    {
        public BloomMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("bloom");
        }

        [Uniform("u_Volume")] public float Volume { get; set; }
    }
}
