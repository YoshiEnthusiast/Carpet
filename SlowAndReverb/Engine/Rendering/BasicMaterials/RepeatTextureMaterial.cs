using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class RepeatTextureMaterial : Material
    {
        public RepeatTextureMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("texturedLine");
        }

        [Uniform("u_Scale")] public Vector2 Scale { get; set; } = Vector2.One;
    }
}
