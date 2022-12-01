using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Layer
    {
        private readonly Texture _renderTarget;
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

            _renderTarget = Texture.CreateEmpty(_width, _height);
        }

        public Layer(int width, int heigth, float depth) : this(width, heigth, depth, new Camera(width, heigth))
        {

        }

        public Texture RenderTarget => _renderTarget;
        public Camera Camera => _camera;
        public int Width => _width;
        public int Height => _height;
        public float Depth => _depth;

        public Material Material { get; set; }
        public Color ClearColor { get; set; }
    }
}
