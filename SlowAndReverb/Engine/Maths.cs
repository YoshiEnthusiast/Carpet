using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    public static class Maths
    {
        public const float PI = 3.141593f;
        public const float TwoPI = PI * 2f;
        public const float HalfPI = PI / 2f;

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

        public static float DeltaAngle(float source, float target)
        {
            float result = target - source;

            while (result > PI)
                result -= TwoPI;

            while (result < -PI)
                result += TwoPI;

            return result;
        }

        public static int Ceiling(float value)
        {
            return (int)Math.Ceiling(value);
        }

        public static int Floor(float value)
        {
            return (int)Math.Floor(value);
        }

        public static float Sqrt(float value)
        {
            return (float)Math.Sqrt(value);
        }

        public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;

            return value;
        }

        public static bool TryGetIntersectionPoint(Vector2 lineOneStart, Vector2 lineOneEnd, Vector2 lineTwoStart, Vector2 lineTwoEnd, out Vector2 intersectionPoint)
        {
            float lineOneX = lineOneStart.X;
            float lineOneY = lineOneStart.Y;
            float lineTwoX = lineTwoStart.X;
            float lineTwoY = lineTwoStart.Y;    

            float lineOneDeltaX = lineOneEnd.X - lineOneX;
            float lineOneDeltaY = lineOneEnd.Y - lineOneY;
            float lineTwoDeltaX = lineTwoEnd.X - lineTwoX;
            float lineTwoDeltaY = lineTwoEnd.Y - lineTwoY;

            float a = lineOneX - lineTwoX;
            float b = lineOneY - lineTwoY;

            float c = -lineTwoDeltaX * lineOneDeltaY + lineOneDeltaX * lineTwoDeltaY;

            float s = (-lineOneDeltaY * a + lineOneDeltaX * b) / c;
            float t = (lineTwoDeltaX * b - lineTwoDeltaY * a) / c;

            if (s >= 0f && s <= 1f && t >= 0f && t <= 1f)
            {
                float x = lineOneX + lineOneDeltaX * t;
                float y = lineOneY + lineOneDeltaY * t;

                intersectionPoint = new Vector2(x, y);

                return true;
            }

            intersectionPoint = default(Vector2);

            return false;
        }

        public static bool TryGetIntersectionPoint(Line lineOne, Line lineTwo, out Vector2 intersectionPoint)
        {
            return TryGetIntersectionPoint(lineOne.Start, lineOne.End, lineTwo.Start, lineTwo.End, out intersectionPoint);
        }
    }
}
