using OpenTK.Mathematics;

namespace Carpet
{
    public abstract class PassBase
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

        public abstract RenderTarget GetRenderTarget();

        public abstract void Process();

        protected void InvokeRenderEvent()
        {
            _render?.Invoke();
        }
    }
}
