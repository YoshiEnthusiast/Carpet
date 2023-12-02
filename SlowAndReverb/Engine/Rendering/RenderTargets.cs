using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public static class RenderTargets
    {
        // TODO: RENAME
        public static RenderTarget LightMap { get; private set; }
        public static RenderTarget ShadowBuffer { get; private set; }
        public static RenderTarget OccluderBuffer { get; private set; }

        internal static void Initialize()
        {
            LightMap = RenderTarget.FromTexture(324, 184);

            ShadowBuffer = RenderTarget.FromTexture(2240, 2240);
            OccluderBuffer = RenderTarget.FromTexture(2240, 2240);
        }
    }
}
