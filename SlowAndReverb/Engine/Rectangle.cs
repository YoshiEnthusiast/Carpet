using System.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    public struct Rectangle
    {
        public static readonly Rectangle Empty = new Rectangle(Vector2.Zero, Vector2.Zero);

        public Rectangle(Vector2 topLeft, Vector2 bottomRight)
        {
            Position = topLeft;
            Size = bottomRight - Position;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            Vector2 topLeft = new Vector2(x, y);

            this = new Rectangle(topLeft, topLeft + new Vector2(width, height));
        }

        public Vector2 Position { get; private init; }
        public Vector2 Size { get; private init; }

        public Vector2 TopLeft => Position;
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => Position + Size;

        public float Left => Position.X;
        public float Top => Position.Y;
        public float Width => Size.X;
        public float Height => Size.Y;

        public float Right => Left + Width;
        public float Bottom => Top + Height;

        public Rectangle Translate(Vector2 by)
        {
            return new Rectangle(Position + by, Size);
        }

        public Rectangle Scale(float by)
        {
            return new Rectangle(Position, Position + Size * by);
        }

        public Rectangle Multiply(float by)
        {
            Vector2 newPosition = Position * by;

            return new Rectangle(newPosition, newPosition + Size * by);
        }

        public Rectangle Divide(Vector2 by)
        {
            Vector2 newPosition = Position / by;

            return new Rectangle(newPosition, newPosition + Size / by);
        }

        public Rectangle Divide(float by)
        {
            return Divide(new Vector2(by));
        }

        public bool Intersects(Rectangle other)
        {
            float otherLeft = other.Left;
            float otherTop = other.Top;
            float otherRight = other.Right;
            float otherBottom = other.Bottom;

            return !(Left < otherLeft && Right <= otherLeft || Right > otherRight && Left >= otherRight
                || Top < otherTop && Bottom <= otherTop || Bottom > otherBottom && Top >= otherBottom);
        }

        public bool TryGetIntersectionRectangle(Rectangle other, out Rectangle result)
        {
            if (!Intersects(other))
            {
                result = default(Rectangle);

                return false;
            } 

            float left = Math.Max(Left, other.Left);
            float top = Math.Max(Top, other.Top);
            float right = Math.Min(Right, other.Right);
            float bottom = Math.Min(Bottom, other.Bottom);

            result = new Rectangle(new Vector2(left, top), new Vector2(right, bottom));

            return true;
        }

        public bool Contains(Rectangle other)
        {
            return other.Left > Left && other.Right < Right && other.Top > Top && other.Bottom < Bottom;
        }

        public bool Contains(Vector2 point)
        {
            float x = point.X;
            float y = point.Y;  

            return x >= Left && x <= Right && y >= Top && y <= Bottom;  
        }

        public Vector4 ToVector4()
        {
            return new Vector4(Position.X, Position.Y, Size.X, Size.Y);
        }

        public float GetArea()
        {
            return Size.X * Size.Y;
        }

        public override string ToString()
        {
            return $"[{TopLeft} {BottomRight}]";
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle && Equals(rectangle);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() + Size.GetHashCode();
        }

        public bool Equals(Rectangle other)
        {
            return Position == other.Position && Size == other.Size;
        }

        public static Rectangle operator *(Rectangle rectangle, float by)
        {
            return rectangle.Multiply(by);
        }

        public static Rectangle operator /(Rectangle rectangle, float by)
        {
            return rectangle.Divide(by);
        }

        public static Rectangle operator /(Rectangle rectangle, Vector2 by)
        {
            return rectangle.Divide(by);
        }
    }
}
