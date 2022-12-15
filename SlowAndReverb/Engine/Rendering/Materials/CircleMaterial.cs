namespace SlowAndReverb
{
    public class CircleMaterial : Material
    {
        public CircleMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("circle");
        }

        [Uniform("u_Width")] public float LineWidth { get; set; } = 1f;
        [Uniform("u_Circumference")] public float Circumference { get; set; } = 1f;
    }
}
