using System;

namespace Carpet
{
    public class SmoothCameraLayer : Layer
    {
        public SmoothCameraLayer(int width, int height, int visibleWidth, int visibleHeight, float depth) 
            : base(width, height, depth)
        {
            VisibleWidth = Maths.Min(visibleWidth, width);
            VisibleHeight = Math.Min(visibleHeight, height);
        }

        public int VisibleWidth { get; private init; }
        public int VisibleHeight { get; private init; }

        public override void Draw(Resolution resolution, SpriteBatch batch)
        {
            var visibleSize = new Vector2(VisibleWidth, VisibleHeight);

            Vector2 scale = resolution.Size / visibleSize;
            float scaleX = scale.X;
            float scaleY = scale.Y;

            float deltaX = (Width - VisibleWidth) * scaleX / 2f;
            float deltaY = (Height - VisibleHeight) * scaleY / 2f;

            float cameraOffsetX = Maths.Fractional(Camera.X) * scaleX;
            float cameraOffsetY = Maths.Fractional(Camera.Y) * scaleY;

            var position = -new Vector2(deltaX + cameraOffsetX, deltaY + cameraOffsetY);

            var bounds = new Rectangle(0f, 0f, Width, Height);

            batch.Submit(RenderTarget.Texture, Material, null, bounds,
                position, scale, Vector2.Zero, Color.White, 0f,
                SpriteEffect.None, SpriteEffect.None, Depth);
        }
    }
}
