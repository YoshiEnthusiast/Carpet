using OpenTK.Mathematics;

namespace Carpet
{
    public abstract class RenderPass : Pass
    {
        public abstract Matrix4? View { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }

        public Vector2 Size => new Vector2(Width, Height);

        public abstract RenderTarget GetRenderTarget();

        public Texture2D GetTexture()
        {
            return GetRenderTarget().Texture;
        }
    }
}
