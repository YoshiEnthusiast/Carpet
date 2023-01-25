using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Layer
    {
        public Layer(int width, int height, float depth, Camera camera)
        {
            Size = new Vector2(width, height);

            Depth = depth;

            Camera = camera;

            Texture texture = Texture.CreateEmpty(width, height);
            RenderTarget = RenderTarget.FromTexture(texture);

            ResetScissor();
        }

        public Layer(int width, int heigth, float depth) : this(width, heigth, depth, new Camera(width, heigth))
        {

        }

        public RenderTarget RenderTarget { get; private init; }
        public Camera Camera { get; private init; }

        public float Depth { get; private init; }
        public Vector2 Size { get; private init; }

        public Vector2 MousePosition => Input.MousePosition * Size / Resolution.CurrentSize;
        public int Width => Size.RoundedX;
        public int Height => Size.RoundedY;

        public Material Material { get; set; }
        public Color ClearColor { get; set; }
        public Rectangle Scissor { get; set; }

        public void ResetScissor()
        {
            Scissor = new Rectangle(0f, 0f, Width, Height);
        }
    }
}
