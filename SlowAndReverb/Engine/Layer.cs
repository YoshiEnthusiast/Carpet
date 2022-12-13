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

        private readonly int _width;
        private readonly int _height;

        private readonly float _depth;

        public Layer(int width, int heigth, float depth, Camera camera)
        {
            _width = width;
            _height = heigth;

            _depth = depth;

            _camera = camera;

            Texture texture = Texture.CreateEmpty(_width, _height); 
            _renderTarget = RenderTarget.FromTexture(texture);

            ResetScissor();
        }

        public Layer(int width, int heigth, float depth) : this(width, heigth, depth, new Camera(width, heigth))
        {

        }

        public RenderTarget RenderTarget => _renderTarget;
        public Camera Camera => _camera;
        public int Width => _width;
        public int Height => _height;
        public float Depth => _depth;

        public Material PostProcessingEffect { get; set; }
        public Color ClearColor { get; set; }
        public Rectangle Scissor { get; set; }

        public void ResetScissor()
        {
            Scissor = new Rectangle(0f, 0f, _width, _height);
        }
    }
}
