using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

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

        public Color(byte r, byte g, byte b)
        {
            this = new Color(r, g, b, byte.MaxValue);
        }

        public Color(int r, int g, int b, int a)
        {
            this = new Color(FromInt(r), FromInt(g), FromInt(b), FromInt(a));   
        }

        public Color(int r, int g, int b)
        {
            this = new Color(r, g, b, byte.MaxValue);
        }

        public Color(float r, float g, float b, float a)
        {
            this = new Color(FromFloat(r), FromFloat(g), FromFloat(b), FromFloat(a));
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

        private byte FromInt(int value)
        {
            return (byte)Math.Clamp(value, byte.MinValue, byte.MaxValue);
        }

        private byte FromFloat(float value)
        {
            return (byte)(byte.MaxValue * value);
        }
    }
}
