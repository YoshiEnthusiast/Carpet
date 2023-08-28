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

        public Texture2D Texture { get; private init; }

        public int Width { get; private init; }
        public int Height { get; private init; }

        public Vector2 Size => new Vector2(Width, Height);

        public static RenderTarget FromTexture(Texture2D texture)
        {
            return new RenderTarget(texture, texture.Width, texture.Height);
        }

        public static RenderTarget FromTexture(int width, int height)
        {
            return FromTexture(Texture2D.CreateEmpty(width, height));
        }

        public static RenderTarget FromScreen(int width, int height)
        {
            return new RenderTarget(null, width, height);
        }
    }
}
