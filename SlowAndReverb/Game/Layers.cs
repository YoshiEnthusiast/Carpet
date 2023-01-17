using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public static class Layers
    {
        private static Layer s_foreground;

        public static Layer Foreground => s_foreground;

        internal static void Initialize()
        {
            s_foreground = new Layer(320, 180, 1f)
            {
                Material = new ForegroundMaterial(),
                ClearColor = new Color(40, 40, 40)
            };
        }
    }
}
