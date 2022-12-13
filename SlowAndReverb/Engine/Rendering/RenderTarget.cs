namespace SlowAndReverb
{
    public sealed class RenderTarget
    {
        private readonly Texture _texture;

        private readonly int _width;
        private readonly int _height;

        private RenderTarget(Texture texture, int width, int height)
        {
            _texture = texture;

            _width = width;
            _height = height;
        }

        public static RenderTarget FromTexture(Texture texture)
        {
            return new RenderTarget(texture, texture.Width, texture.Height);
        }

        public static RenderTarget FromScreen(int width, int height)
        {
            return new RenderTarget(null, width, height);
        }

        public Texture Texture => _texture;

        public int Width => _width;
        public int Height => _height;
    }
}
