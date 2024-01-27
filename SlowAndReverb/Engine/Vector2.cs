using System;
using System.Runtime.CompilerServices;

namespace Carpet
{
    public struct Vector2
    {
        public static readonly Vector2 Zero = new Vector2();
        public static readonly Vector2 One = new Vector2(1f);
        public static readonly Vector2 XUnit = new Vector2(1f, 0f);
        public static readonly Vector2 YUnit = new Vector2(0f, 1f);

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(float value)
        {
            this = new Vector2(value, value);
        }

        public float X { get; init; }    
        public float Y { get; init; }

        public int FlooredX => Maths.Floor(X);
        public int FlooredY => Maths.Floor(Y);

        public static float Distance(Vector2 from, Vector2 to)
        {
            Vector2 quotient = from - to;

            return quotient.GetMagnitude();
        }

        public static Vector2 FromVector2GL(Vector2GL vector)
        {
            return Unsafe.As<Vector2GL, Vector2>(ref vector);
        }

        public static Vector2 Lerp(Vector2 vector, Vector2 destination, float amount)
        {
            return vector.Lerp(destination, amount);
        }

        public static Vector2 LerpSmooth(Vector2 vector, Vector2 destination, float multiplier)
        {
            float distance = Distance(vector, destination);

            return Lerp(vector, destination, distance * multiplier);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2 vector && Equals(vector); 
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public bool Equals(Vector2 vector)
        {
            return X == vector.X && Y == vector.Y;
        }

        public Vector2 Add(Vector2 value)
        {
            return new Vector2(X + value.X, Y + value.Y);   
        }

        public Vector2 AddX(float value)
        {
            return new Vector2(X + value, Y);
        }

        public Vector2 AddY(float value)
        {
            return new Vector2(X, Y + value);
        }

        public Vector2 Subtract(Vector2 value)
        {
            return Add(value.Negate());
        }

        public Vector2 Multiply(float by)
        {
            return new Vector2(X * by, Y * by);
        }

        public Vector2 Multiply(Vector2 by)
        {
            return new Vector2(X * by.X, Y * by.Y);
        }

        public Vector2 Divide(float by)
        {
            return new Vector2(X / by, Y / by);
        }

        public Vector2 Divide(Vector2 by)
        {
            return new Vector2(X / by.X, Y / by.Y);
        }

        public Vector2 Rotate(Vector2 pivot, float angle)
        {
            float sin = Maths.Sin(angle);
            float cos = Maths.Cos(angle);

            float pivotX = pivot.X;
            float pivotY = pivot.Y;
            float deltaX = X - pivotX;
            float deltaY = Y - pivotY;

            float x = deltaX * cos - deltaY * sin + pivotX;
            float y = deltaX * sin + deltaY * cos + pivotY;

            return new Vector2(x, y);
        }

        public Vector2 Rotate(float angle)
        {
            return Rotate(Zero, angle);
        }

        public Vector2 Floor()
        {
            return new Vector2(FlooredX, FlooredY);
        }

        public Vector2 Ceiling()
        {
            return new Vector2(Maths.Ceiling(X), Maths.Ceiling(Y));
        }

        public Vector2 Negate()
        {
            return new Vector2(-X, -Y);
        }

        public Vector2 Normalize()
        {
            return new Vector2(X, Y) / GetMagnitude();
        }

        public Vector2 Fractional()
        {
            float x = Maths.Fractional(X);
            float y = Maths.Fractional(Y);

            return new Vector2(x, y);
        }

        public float GetMagnitude()
        {
            return Maths.Sqrt(Maths.Pow(X, 2) + Maths.Pow(Y, 2));
        }

        public Vector2 Lerp(Vector2 destination, float amount)
        {
            float x = Maths.Lerp(X, destination.X, amount);
            float y = Maths.Lerp(Y, destination.Y, amount);

            return new Vector2(x, y);
        }

        public Vector2 Clamp(Vector2 min, Vector2 max)
        {
            float x = Maths.Clamp(X, min.X, max.X);
            float y = Maths.Clamp(Y, min.Y, max.Y);

            return new Vector2(x, y);
        }

        public Vector2 Clamp(float min, float max)
        {
            return Clamp(new Vector2(min), new Vector2(max));
        }

        public Vector2GL ToVector2GL()
        {
            return Unsafe.As<Vector2, Vector2GL>(ref this);
        }

        public override string ToString()
        {
            return $"(Vector2) [{X} {Y}]";
        }

        public static Vector2 operator +(Vector2 vector, Vector2 other)
        {
            return vector.Add(other);
        }

        public static Vector2 operator -(Vector2 vector, Vector2 other)
        {
            return vector.Subtract(other);
        }

        public static Vector2 operator *(Vector2 vector, float multiplyBy)
        {
            return vector.Multiply(multiplyBy);
        }

        public static Vector2 operator *(Vector2 vector, Vector2 multiplyBy)
        {
            return vector.Multiply(multiplyBy);
        }

        public static Vector2 operator /(Vector2 vector, float divideBy)
        {
            return vector.Divide(divideBy);
        }

        public static Vector2 operator /(Vector2 vector, Vector2 divideBy)
        {
            return vector.Divide(divideBy);
        }

        public static Vector2 operator -(Vector2 vector)
        {
            return vector.Negate();
        }

        public static bool operator ==(Vector2 vector, Vector2 other)
        {
            return vector.Equals(other);  
        }

        public static bool operator !=(Vector2 vector, Vector2 other)
        {
            return !vector.Equals(other);
        }

        public static implicit operator Vector2GL (Vector2 vector)
        {
            return vector.ToVector2GL();
        }

        public static implicit operator Vector2 (Vector2GL vector)
        {
            return FromVector2GL(vector);
        }
    }
}
