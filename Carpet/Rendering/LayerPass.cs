using OpenTK.Mathematics;

namespace Carpet
{
    public class LayerPass : RenderPass
    {
        private Layer _layer;

        public Layer GetLayer()
        {
            return _layer;
        }

        public override Matrix4? View
        {
            get
            {
                return _layer.Camera.GetViewMatrix();
            }
        }

        public override int Width
        {
            get
            {
                return _layer.Width;
            }
        }

        public override int Height
        {
            get
            {
                return _layer.Height;
            }
        }

        public LayerPass SetLayer(Layer layer)
        {
            _layer = layer;

            return this;
        }

        public override RenderTarget GetRenderTarget()
        {
            return _layer.RenderTarget;
        }

        public override void Process()
        {
            Graphics.BeginLayer(_layer);
            InvokeProcessEvent();
            Graphics.EndCurrentLayer();
        }
    }
}
