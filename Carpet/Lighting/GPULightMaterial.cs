namespace Carpet
{
    public class GPULightMaterial : Material
    {
        public GPULightMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("GPULight.frag");
        }

        [Uniform("u_Resolution")] public Vector2 Resolution { get; set; }

        [Uniform("u_Index")] public float Index { get; set; }

        [Uniform("u_Rotation")] public float Rotation { get; set; }
        [Uniform("u_Angle")] public float Angle { get; set; } = Maths.TwoPI;
        [Uniform("u_FalloffAngle")] public float FalloffAngle { get; set; }

        [Uniform("u_StartDistance")] public float StartDistance { get; set; }
        [Uniform("u_StartFade")] public float StartFade { get; set; }

        [Uniform("u_Volume")] public float Volume { get; set; }

        [Uniform("u_Glare")] public bool Glare { get; set; }
    }
}
