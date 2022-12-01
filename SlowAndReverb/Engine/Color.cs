using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public struct Color
    {
        public static readonly Color White = new Color(255, 255, 255, 255);

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R { get; init; }
        public byte G { get; init; }
        public byte B { get; init; }
        public byte A { get; init; }

        public Vector4 ToVector4()
        {
            float r = ConvertToFloat(R);
            float g = ConvertToFloat(G);
            float b = ConvertToFloat(B);
            float a = ConvertToFloat(A);

            return new Vector4(r, g, b, a);
        }

        public Color4 ToColor4()
        {
            return new Color4(R, G, B, A);
        }

        private float ConvertToFloat(byte component)
        {
            return component / 255f;
        }
    }
}
