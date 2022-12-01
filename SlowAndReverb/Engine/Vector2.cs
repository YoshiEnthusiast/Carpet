﻿using System;
using System.Runtime.CompilerServices;

namespace SlowAndReverb
{
    public struct Vector2
    {
        public static readonly Vector2 Zero = new Vector2();
        public static readonly Vector2 One = new Vector2(1f);

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

        public int RoundedX => (int)X;
        public int RoundedY => (int)Y;

        public static float Distance(Vector2 from, Vector2 to)
        {
            Vector2 quotient = from - to;

            return quotient.GetMagnitude();
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

        public Vector2 Subtract(Vector2 value)
        {
            return new Vector2(X - value.X, Y - value.Y);
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

        public Vector2 Round()
        {
            return new Vector2(RoundedX, RoundedY);
        }

        public Vector2 Negate()
        {
            return new Vector2(-X, -Y);
        }

        public float GetMagnitude()
        {
            return (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        }

        public Vector2GL ToVector2GL()
        {
            return Unsafe.As<Vector2, Vector2GL>(ref this);
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
    }
}