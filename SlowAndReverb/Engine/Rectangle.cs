using System.Drawing;
using System;

namespace SlowAndReverb
{
    public struct Rectangle
    {
        public static readonly Rectangle Empty = new Rectangle(Vector2.Zero, Vector2.Zero);

        private readonly Vector2 _position;
        private readonly Vector2 _size;

        public Rectangle(Vector2 topLeft, Vector2 bottomRight)
        {
            _position = topLeft;
            _size = bottomRight - _position;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            Vector2 topLeft = new Vector2(x, y);

            this = new Rectangle(topLeft, topLeft + new Vector2(width, height));
        }

        public Vector2 Position => _position;
        public Vector2 Size => _size;

        public Vector2 TopLeft => _position;
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => _position + _size;

        public float Left => _position.X;
        public float Top => _position.Y;
        public float Width => _size.X;
        public float Height => _size.Y;

        public float Right => Left + Width;
        public float Bottom => Top + Height;

        public Rectangle Translate(Vector2 by)
        {
            return new Rectangle(_position + by, _size);
        }

        public Rectangle Scale(float by)
        {
            return new Rectangle(_position, _size * by);
        }

        public bool Intersects(Rectangle other)
        {
            float otherX = other.Left;
            float otherY = other.Top;

            return Left < otherX && Right > otherX || otherX < Left && other.Right > Left ||
                Top < otherY && Bottom > otherY || otherY < Top && other.Bottom > Top;
        }

        public bool Contains(Rectangle other)
        {
            return other.Left > Left && other.Right < Right && other.Top > Top && other.Bottom < Bottom;
        }

        public float GetArea()
        {
            return _size.X * _size.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle && Equals(rectangle);
        }

        public override int GetHashCode()
        {
            return _position.GetHashCode() + _size.GetHashCode();
        }

        public bool Equals(Rectangle other)
        {
            return _position == other.Position && _size == other.Size;
        }
    }
}
