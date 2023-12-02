using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public class Pass : PassBase
    {
        private RenderTarget _target;

        public Pass(BlendMode blendMode, Color clearColor)
        {
            BlendMode = blendMode;
            ClearColor = clearColor;
        }

        public BlendMode BlendMode { get; set; }
        public Color ClearColor { get; set; }
        public Matrix4? View { get; set; }

        public override RenderTarget GetRenderTarget()
        {
            return _target;
        }

        public Pass SetRenderTarget(RenderTarget target)
        {
            _target = target;

            return this;
        }

        public override void Process()
        {
            SpriteBatch batch = Graphics.SpriteBatch;

            batch.Begin(_target, BlendMode, ClearColor, View);
            InvokeRenderEvent();
            batch.End();
        }
    }
}
