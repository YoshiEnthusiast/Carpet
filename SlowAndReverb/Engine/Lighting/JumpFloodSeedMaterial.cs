using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class JumpFloodSeedMaterial : Material
    {
        public JumpFloodSeedMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("jumpFloodSeed");
        }

        [Uniform("u_Resolution")] public Vector2 Resolution { get; set; }
    }
}
