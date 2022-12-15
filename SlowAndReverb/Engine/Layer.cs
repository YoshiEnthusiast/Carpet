using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Layer
    {
        private readonly RenderTarget _renderTarget;
        private readonly Camera _camera;

        private readonly Vector2 _size;
        private readonly float _depth;

        public Layer(int width, int height, float depth, Camera camera)
        {
            _size = new Vector2(width, height);

            _depth = depth;

            _camera = camera;

            Texture texture = Texture.CreateEmpty(width, height); 
            _renderTarget = RenderTarget.FromTexture(texture);

            ResetScissor();
        }

        public Layer(int width, int heigth, float depth) : this(width, heigth, depth, new Camera(width, heigth))
        {

        }

        public RenderTarget RenderTarget => _renderTarget;
        public Camera Camera => _camera;

        public Vector2 MousePosition => Input.MousePosition * _size / Resolution.CurrentSize;
        public Vector2 Size => _size;
        public int Width => _size.RoundedX;
        public int Height => _size.RoundedY;
        public float Depth => _depth;

        public Material PostProcessingEffect { get; set; }
        public Color ClearColor { get; set; }
        public Rectangle Scissor { get; set; }

        public void ResetScissor()
        {
            Scissor = new Rectangle(0f, 0f, Width, Height);
        }
    }
}
