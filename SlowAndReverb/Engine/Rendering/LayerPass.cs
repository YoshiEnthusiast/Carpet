namespace SlowAndReverb
{
    public class LayerPass : PassBase
    {
        private Layer _layer;

        public Layer GetLayer()
        {
            return _layer;
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
            InvokeRenderEvent();
            Graphics.EndCurrentLayer();
        }
    }
}
