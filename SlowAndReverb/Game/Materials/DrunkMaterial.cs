namespace SlowAndReverb
{
    public class DrunkMaterial : Material
    {
        public DrunkMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("drunk");
        }

        [Uniform("u_Time")] public float Time { get; set; }
        [Uniform("u_MaxTilt")] public int MaxTilt { get; set; }
    }
}
