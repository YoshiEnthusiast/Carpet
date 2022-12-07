namespace SlowAndReverb
{
    public struct Rectangle
    {
        private readonly Vector2 _position;
        private readonly Vector2 _size;

        public Rectangle(Vector2 position, Vector2 size)
        {
            _position = position;
            _size = size;
        }

        public Rectangle(float x, float y, float width, float height) : this(new Vector2(x, y), new Vector2(width, height))
        {

        }

        public Vector2 Position => _position;
        public Vector2 Size => _size;

        public Vector2 TopLeft => _position;
        public Vector2 TopRight => new Vector2(Right, Y);
        public Vector2 BottomLeft => new Vector2(X, Bottom);
        public Vector2 BottomRight => _position + _size;

        public float X => _position.X;
        public float Y => _position.Y;
        public float Width => _size.X;
        public float Height => _size.Y;

        public float Right => X + Width;
        public float Bottom => Y + Height;

        public bool Intersects(Rectangle other)
        {
            float otherX = other.X;
            float otherY = other.Y;

            return X < otherX && Right > otherX || otherX < X && other.Right > X ||
                Y < otherY && Bottom > otherY || otherY < Y && other.Bottom > Y;
        }

        public bool Contains(Rectangle other)
        {
            return other.X > X && other.Right < Right && other.Y > Y && other.Bottom < Bottom;
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
