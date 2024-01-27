using OpenTK.Mathematics;

namespace Carpet
{
    public class LightMaterial : Material
    {
        public LightMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("light");

            // TODO: remove dependency from UntitledGame
            Textures.Add(Demo.ShadowBuffer.Texture);
            Textures.Add(Demo.OccludeBuffer.Texture);
        }

        [Uniform("u_ShadowBounds")] public Vector4 ShadowBounds { get; set; }
        [Uniform("u_ShadowTexRes")] public Vector2 ShadowTextureResolution { get; set; }
        [Uniform("u_Mask")] public float Mask { get; set; }

        [Uniform("u_Rotation")] public float Rotation { get; set; }
        [Uniform("u_Angle")] public float Angle { get; set; } = Maths.TwoPI;
        [Uniform("u_FalloffAngle")] public float FalloffAngle { get; set; }

        [Uniform("u_StartDistance")] public float StartDistance { get; set; }
        [Uniform("u_StartFade")] public float StartFade { get; set; }

        [Uniform("u_Volume")] public float Volume { get; set; }

        [Uniform("u_ShadowFalloff")] public float ShadowFalloff { get; set; } = 1f;
    }
}
