using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TextMaterial : Material
    {
        public TextMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("text");
        }

        [Uniform("u_Color")] public Color Color { get; set; } 
        [Uniform("u_OutlineColor")] public Color OutlineColor { get; set; }
        [Uniform("u_OutlineWidth")] public int OutlineWidth { get; set; }
    }
}
