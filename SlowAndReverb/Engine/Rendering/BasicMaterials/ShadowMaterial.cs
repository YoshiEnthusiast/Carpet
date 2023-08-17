﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class ShadowMaterial : Material
    {
        public ShadowMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("shadow");
        }
    }
}
