using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Layer
    {
        public Layer(int width, int height, float depth)
        {
            Size = new Vector2(width, height);

            Depth = depth;

            Camera = new Camera(width, height);

            Texture2D texture = Texture2D.CreateEmpty(width, height);
            RenderTarget = RenderTarget.FromTexture(texture);

            ResetScissor();
        }

        public Vector2 MousePosition
        {
            get
            {
                Vector2 screenPosition = Input.MousePosition * Size / Resolution.CurrentSize;

                return screenPosition + Camera.Position - Camera.Origin;
            }
        }

        public RenderTarget RenderTarget { get; private init; }
        public Camera Camera { get; private init; }

        public float Depth { get; private init; }
        public Vector2 Size { get; private init; }

        public int Width => Size.FlooredX;
        public int Height => Size.FlooredY;

        public Material Material { get; set; }
        public Color ClearColor { get; set; }
        public Rectangle Scissor { get; set; }

        public virtual void Draw(Resolution resolution, SpriteBatch batch)
        {
            Vector2 scale = resolution.Size / Size;

            var bounds = new Rectangle(0f, 0f, Width, Height);

            batch.Submit(RenderTarget.Texture, Material, null, bounds, 
                Vector2.Zero, scale, Vector2.Zero, Color.White, 0f, 
                SpriteEffect.None, SpriteEffect.None, Depth);
        }

        public void ResetScissor()
        {
            Scissor = new Rectangle(0f, 0f, Width, Height);
        }
    }
}
