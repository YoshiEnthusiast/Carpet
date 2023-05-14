using System;
using System.Numerics;

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

        public static float Max(float value1, float value2)
        {
            return Math.Max(value1, value2);
        }

        public static float Min(float value1, float value2)
        {
            return Math.Min(value1, value2);
        }

        public static int Max(int value1, int value2)
        {
            return Math.Max(value1, value2);
        }

        public static int Min(int value1, int value2)
        {
            return Math.Min(value1, value2);
        }

        public static float Abs(float value)
        {
            return Math.Abs(value);
        }

        public static int Sign(float value)
        {
            return Math.Sign(value);
        }

        public static float Pow(float value, float exponent)
        {
            return (float)Math.Pow(value, exponent);
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

            intersectionPoint = default;

            return false;
        }

        public static bool TryGetIntersectionPoint(Line lineOne, Line lineTwo, out Vector2 intersectionPoint)
        {
            return TryGetIntersectionPoint(lineOne.Start, lineOne.End, lineTwo.Start, lineTwo.End, out intersectionPoint);
        }

        public static float Lerp(float value, float destination, float amount)
        {
            amount = Clamp(amount, 0f, 1f);
            
            return value + (destination - value) * amount;
        }

        public static bool WithinCircle(Vector2 position, Vector2 center, float radius)
        {
            float distance = Vector2.Distance(center, position);

            if (distance <= radius)
                return true;

            return false;
        }

        public static bool WithinCircle(Rectangle rectangle, Vector2 center, float radius)
        {
            float entityRight = rectangle.Right;
            float entityBottom = rectangle.Bottom;

            float closestX = center.X > entityRight ? entityRight : rectangle.Left;
            float closestY = center.Y > entityBottom ? entityBottom : rectangle.Top;

            return WithinCircle(new Vector2(closestX, closestY), center, radius);
        }

        public static float Approach(float current, float value, float max)
        {
            float result = current + value;

            return Min(result, max);
        }

        public static float ApproachAbs(float current, float value, float max)
        {
            float result = current + value;

            return Clamp(result, -max, max);
        } 
    }
}
