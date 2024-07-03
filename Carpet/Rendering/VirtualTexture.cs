namespace Carpet
{
    public sealed class VirtualTexture
    {
        private readonly Rectangle _bounds;

        public VirtualTexture(Texture2D actualTexture, Rectangle bounds)
        {
            ActualTexture = actualTexture;

            _bounds = bounds;
        }

        public Texture2D ActualTexture { get; private init; }

        public Vector2 Size => _bounds.Size;

        public int Width => (int)_bounds.Width;
        public int Height => (int)_bounds.Height;

        public Rectangle GetBounds(Rectangle localBounds)
        {
            Vector2 position = _bounds.Position + localBounds.Position;

            return new Rectangle(position, position + localBounds.Size);
        }

        public Rectangle GetBounds()
        {
            return _bounds;
        }
    }
}
