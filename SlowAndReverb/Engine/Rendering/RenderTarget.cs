namespace SlowAndReverb
{
    public sealed class RenderTarget
    {
        private RenderTarget(Texture2D texture, int width, int height)
        {
            Texture = texture;

            Width = width;
            Height = height;
        }

        public static RenderTarget FromTexture(Texture2D texture)
        {
            return new RenderTarget(texture, texture.Width, texture.Height);
        }

        public static RenderTarget FromScreen(int width, int height)
        {
            return new RenderTarget(null, width, height);
        }

        public Texture2D Texture { get; private init; }

        public int Width { get; private init; }
        public int Height { get; private init; }
    }
}
