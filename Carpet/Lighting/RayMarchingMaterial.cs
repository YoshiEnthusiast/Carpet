namespace Carpet
{
    public class RayMarchingMaterial : Material
    {
        public RayMarchingMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("rayMarch");
        }

        [Uniform("u_RaysPerPixel")] public int RaysPerPixel { get; set; } = 50;
        [Uniform("u_MaxSteps")] public int MaxSteps { get; set; } = 100;
        [Uniform("u_SurfaceDistance")] public float SurfaceDistance { get; set; } = 0.01f;
        [Uniform("u_MaxDistance")] public float MaxDistance { get; set; } = 500f;
        [Uniform("u_Time")] public float Time { get; set; }
    }
}
