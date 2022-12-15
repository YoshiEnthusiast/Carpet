using System;
using System.Numerics;

namespace SlowAndReverb
{
    public static class Maths
    {
        public const float PI = 3.141593f;
        public const float TwoPI = PI * 2f;

        public static float Sin(float angle)
        {
            return (float)Math.Sin(angle);
        }

        public static float Cos(float angle)
        {
            return (float)Math.Cos(angle);
        }

        public static float Tan(float angle)
        {
            return (float)Math.Tan(angle);    
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Atan2(Vector2 vector, Vector2 other)
        {
            float deltaY = other.Y - vector.Y;
            float deltaX = other.X - vector.X;

            return Atan2(deltaY, deltaX);
        }

        public static int Ceiling(float value)
        {
            return (int)Math.Ceiling(value);
        }

        public static int Floor(float value)
        {
            return (int)Math.Floor(value);
        }

        public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;

            return value;
        }
    }
}
