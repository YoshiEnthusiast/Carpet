using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public static class RenderTargets
    {
        public static RenderTarget LightMap { get; private set; }
        public static RenderTarget ShadowBuffer { get; private set; }

        internal static void Initialize()
        {
            LightMap = RenderTarget.FromTexture(Texture2D.CreateEmpty(320, 180));

            ShadowBuffer = RenderTarget.FromTexture(Texture2D.CreateEmpty(2240, 2240));
        }
    }
}
