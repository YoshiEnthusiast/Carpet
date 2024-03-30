using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class DistanceMapMaterial : Material
    {
        public DistanceMapMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("distanceMap.frag");
        }

        [Uniform("u_LightPosition")] public Vector2 LightPosition { get; set; }
        [Uniform("u_Radius")] public float Radius { get; set; }
    }
}
