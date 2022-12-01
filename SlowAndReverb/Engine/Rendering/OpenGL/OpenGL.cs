using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public static class OpenGL
    {
        private static int s_maxTextureUnits;

        public static void Initialize()
        {
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.GetInteger(GetPName.MaxTextureImageUnits, out s_maxTextureUnits);
        }

        public static int MaxTextureUnits => s_maxTextureUnits;
    }
}
