using OpenTK.Mathematics;

namespace Carpet
{
    public abstract class RenderPass
    {
        private Render _render;

        public event Render Render
        {
            add
            {
                _render += value;
            }

            remove
            {
                _render -= value;
            }
        }

        public abstract Matrix4? View { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }

        public Vector2 Size => new Vector2(Width, Height);

        public abstract RenderTarget GetRenderTarget();

        public abstract void Process();

        public Texture2D GetTexture()
        {
            return GetRenderTarget().Texture;
        }

        protected void InvokeRenderEvent()
        {
            _render?.Invoke();
        }
    }
}
