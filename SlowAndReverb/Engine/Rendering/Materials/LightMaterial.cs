using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class LightMaterial : Material
    {
        public LightMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("light");

            Textures.Add(RenderTargets.ShadowBuffer.Texture);
        }

        [Uniform("u_ShadowBounds")] public Vector4 ShadowBounds { get; set; }
        [Uniform("u_Mask")] public float Mask { get; set; }
    }
}
