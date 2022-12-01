using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public sealed class TestMaterial : Material
    {
        public TestMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("test");
        }

        [Uniform("u_Time")] public float Time { get; set; }
    }
}
