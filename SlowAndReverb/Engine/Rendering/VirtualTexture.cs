using System;

namespace SlowAndReverb
{
    public sealed class VirtualTexture
    {
        private readonly Rectangle _bounds;

        public VirtualTexture(Texture actualTexture, Rectangle bounds)
        {
            ActualTexture = actualTexture;

            _bounds = bounds;
        }

        public Texture ActualTexture { get; private init; }

        public Vector2 Size => _bounds.Size;

        public int Width => (int)_bounds.Width;
        public int Height => (int)_bounds.Height;

        public Rectangle GetBounds(Rectangle localBounds)
        {
            // Make it do that you get go beyond _bounds

            Vector2 position = _bounds.Position + localBounds.Position;

            return new Rectangle(position, position + localBounds.Size);
        }

        public Rectangle GetBounds()
        {
            return _bounds;
        }
    }
}
