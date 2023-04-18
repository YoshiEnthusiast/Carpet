using OpenTK.Mathematics;
using System;

namespace SlowAndReverb
{
    public struct Color
    {
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Blue = new Color(0, 0, 255);
        public static readonly Color LightGreen = new Color(85, 200, 85);
        public static readonly Color DarkGreen = new Color(45, 110, 8);
        public static readonly Color Pink = new Color(255, 0, 127);
        public static readonly Color Yellow = new Color(255, 255, 0);
        public static readonly Color CoolOrange = new Color(233, 129, 46);
        public static readonly Color Grey = new Color(177, 177, 177);

        public static readonly Color Transparent = new Color(0, 0, 0, 0);

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

        public Color(float r, float g, float b)
        {
            this = new Color(r, g, b, byte.MaxValue);
        }

        public Color Lerp(Color destination, float amount)
        {
            byte R = FromFloat(Maths.Lerp(NormalizedR, destination.NormalizedR, amount));
            byte G = FromFloat(Maths.Lerp(NormalizedG, destination.NormalizedG, amount));
            byte B = FromFloat(Maths.Lerp(NormalizedB, destination.NormalizedB, amount));
            byte A = FromFloat(Maths.Lerp(NormalizedA, destination.NormalizedA, amount));

            return new Color(R, G, B, A);
        }

        public byte R { get; init; }
        public byte G { get; init; }
        public byte B { get; init; }
        public byte A { get; init; }

        public float NormalizedR => ConvertToFloat(R);
        public float NormalizedG => ConvertToFloat(G);
        public float NormalizedB => ConvertToFloat(B);
        public float NormalizedA => ConvertToFloat(A);

        public Vector4 ToVector4()
        {
            return new Vector4(NormalizedR, NormalizedG, NormalizedB, NormalizedA);
        }

        public Color4 ToColor4()
        {
            return new Color4(R, G, B, A);
        }

        public static Color Lerp(Color color, Color destination, float amount)
        {
            return color.Lerp(destination, amount);
        }

        public static Color operator *(Color color, float value)
        {
            return new Color(Multiply(color.R, value), Multiply(color.G, value), Multiply(color.B, value), Multiply(color.A, value));
        }
        private static byte Multiply(byte value, float by)
        {
            return (byte)Maths.Clamp(value * by, 0f, 255f);
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
