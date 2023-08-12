using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace SlowAndReverb
{
    public sealed class Palette
    {
        private const int DefaultTextureSize = 64;

        public Palette(Stream stream, int textureSize)
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            byte[] data = image.Data;

            var colors = new List<Color>();
            var colorsLAB = new List<LAB>();

            int colorComponentsCount = Color.ComponentsCount;

            for (int i = 0; i < data.Length; i += colorComponentsCount)
            {
                byte r = data[i];
                byte g = data[i + 1];
                byte b = data[i + 2];

                var color = new Color(r, g, b);

                if (!colors.Contains(color))
                {
                    colors.Add(color);

                    LAB colorLAB = LAB.FromRGB(color);
                    colorsLAB.Add(colorLAB);
                }
            }

            var texture = new Texture3D(textureSize, textureSize, textureSize);
            var textureData = new byte[(int)Math.Pow(textureSize, 3f) * colorComponentsCount];

            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    for (int z = 0; z < textureSize; z++)
                    {
                        float r = NormalizeCoordinate(x, textureSize);
                        float g = NormalizeCoordinate(y, textureSize);
                        float b = NormalizeCoordinate(z, textureSize);

                        var color = new Color(r, g, b);
                        LAB colorLAB = LAB.FromRGB(color);

                        float minDistance = float.MaxValue;
                        int index = 0;

                        for (int i = 0; i < colorsLAB.Count; i++)
                        {
                            LAB paletteLAB = colorsLAB[i];
                            float distance = GetDistance(colorLAB, paletteLAB);

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                index = i;
                            }
                        }

                        Color paletteColor = colors[index];

                        byte paletteR = paletteColor.R;
                        byte paletteG = paletteColor.G;
                        byte paletteB = paletteColor.B;
                        byte paletteA = paletteColor.A;

                        int dataIndex = (z * textureSize * textureSize + y * textureSize + x)
                            * colorComponentsCount;

                        textureData[dataIndex] = paletteR;
                        textureData[dataIndex + 1] = paletteG;
                        textureData[dataIndex + 2] = paletteB;
                        textureData[dataIndex + 3] = paletteA;
                    }
                }
            }

            texture.SetData(textureData);

            Texture = texture;
        }

        public Palette(Stream stream) : this(stream, DefaultTextureSize)
        {
            
        }

        public Texture3D Texture { get; private init; }


        private float GetDistance(LAB lab1, LAB lab2)
        {
            float deltaL = lab1.L - lab2.L;
            float deltaA = lab1.A - lab2.A;
            float deltaB = lab1.B - lab2.B;

            return Maths.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
        }

        private float GetDistanceCIE94(LAB lab1, LAB lab2)
        {
            float l1 = lab1.L;
            float a1 = lab1.A;
            float b1 = lab1.B;

            float l2 = lab2.L;
            float a2 = lab2.A;
            float b2 = lab2.B;

            float deltaL = l1 - l2;
            float deltaA = a1 - a2;
            float deltaB = b1 - b2;

            float c1 = Maths.Sqrt(a1 * a1 + b1 * b1);
            float c2 = Maths.Sqrt(a2 * a2 + b2 * b2);

            float deltaC = c1 - c2;
            float h = Sqrt(deltaA * deltaA + deltaB * deltaB - deltaC * deltaC);

            float sL = 1f;
            float kL = 1f;
            float kH = 1f;
            float kc = 1f;
            float sC = 1f + c1 * 0.045f;
            float sH = 1f + c1 * 0.015f;

            float e1 = deltaL / (kL * sL);
            float e2 = deltaC / (kc * sC);
            float e3 = h / (kH * sH);

            return Sqrt(e1 * e1 + e2 * e2 + e3 * e3);
        }

        private float Sqrt(float value)
        {
            if (value < 0f)
                return 0f;

            return Maths.Sqrt(value);
        }

        private float NormalizeCoordinate(float coordinate, float size)
        {
            return coordinate / size;
        }

        private struct LAB
        {
            public LAB(float l, float a, float b)
            {
                L = l;
                A = a;
                B = b;
            }

            public float L { get; private init; }
            public float A { get; private init; }
            public float B { get; private init; }

            public static LAB FromRGB(Color color)
            {
                float r = color.RNormalized;
                float g = color.GNormalized;
                float b = color.BNormalized;

                r = GetCLinear(r);
                g = GetCLinear(g);
                b = GetCLinear(b);

                float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
                float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
                float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

                x /= 97.285f;
                y /= 100f;
                z /= 116.145f;

                x = F(x);
                y = F(y);
                z = F(z);

                float l = (116f * y) - 16f;
                float a = 500f * (x - y);
                b = 200f * (y - z);

                return new LAB(l, a, b);
            }

            private static float GetCLinear(float c)
            {
                if (c > 0.04045f)
                {
                    c = (c + 0.055f) / 1.055f;

                    return Maths.Pow(c, 2.4f) * 100f;
                }

                return c / 12.92f * 100f;
            }

            private static float F(float c)
            {
                if (c > 0.008856f)
                    return Maths.Pow(c, 1f / 3f);

                return (c * 7.787f + 16f / 116f);
            }
        }
    }
}
