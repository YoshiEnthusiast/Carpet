using OpenTK.Mathematics;

namespace Carpet
{
    public class TexturePass : RenderPass
    {
        private RenderTarget _target;

        private Matrix4? _view;

        public TexturePass(BlendMode blendMode, Color clearColor, Matrix4 view)
        {
            BlendMode = blendMode;
            ClearColor = clearColor;
            _view = view;
        }

        public TexturePass(BlendMode blendMode, Color clearColor) : this(blendMode, clearColor,
            Matrix4.Identity)
        {

        }

        public TexturePass(BlendMode blendMode) : this(blendMode, Color.Transparent)
        {

        }

        public TexturePass() : this(BlendMode.AlphaBlend)
        {

        }

        public override Matrix4? View
        {
            get
            {
                return _view;
            }
        }

        public override int Width
        {
            get 
            {
                return _target.Width;
            }
        }

        public override int Height
        {
            get
            {
                return _target.Height;
            }
        }

        public Color ClearColor { get; set; }
        public BlendMode BlendMode { get; set; }

        public override RenderTarget GetRenderTarget()
        {
            return _target;
        }

        public TexturePass SetRenderTarget(RenderTarget target)
        {
            _target = target;

            return this;
        }

        public TexturePass SetView(Matrix4 view)
        {
            _view = view;

            return this;
        }

        public override void Process()
        {
            SpriteBatch batch = Graphics.SpriteBatch;

            batch.Begin(_target, BlendMode, ClearColor, View);
            InvokeProcessEvent();
            batch.End();
        }
    }
}
