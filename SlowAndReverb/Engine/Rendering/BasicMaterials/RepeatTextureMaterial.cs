using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class RepeatTextureMaterial : Material
    {
        public RepeatTextureMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("repeat");
        }

        [Uniform("u_Scale")] public Vector2 Scale { get; set; } = Vector2.One;
        [Uniform("u_Scroll")] public Vector2 Scroll { get; set; }
    }
}
