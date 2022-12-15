using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public static class Graphics
    {
        private static readonly List<Layer> s_drawnLayers = new List<Layer>();
        private static readonly SpriteBatch s_spriteBatch = new SpriteBatch(true);

        private static readonly Texture s_blankTexture = Content.GetTexture("blank");

        private static RenderTarget s_finalTarget;
        private static RenderTarget s_screenTarget;

        private static Layer s_currentLayer;

        public static Layer CurrentLayer => s_currentLayer;

        public static Material PostProcessingEffect { get; set; }
        public static Color ClearColor { get; set; }
        public static Rectangle Scissor { get; set; }

        public static void Initialize()
        {
            Resolution.Change += OnResolutionChnaged;

            ApplyResolution();
        }

        public static void Draw(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            s_spriteBatch.Submit(texture, material, bounds, position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        public static void Draw(Texture texture, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Draw(texture, null, bounds, position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        public static void Draw(Texture texture, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Draw(texture, bounds, position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public static void Draw(Texture texture, Material material, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Draw(texture, material, new Rectangle(Vector2.Zero, new Vector2(texture.Width, texture.Height)), position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        public static void Draw(Texture texture, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Draw(texture, new Rectangle(Vector2.Zero, new Vector2(texture.Width, texture.Height)), position, scale, origin, color, angle, depth);
        }

        public static void Draw(Texture texture, Material material, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Draw(texture, material, position, scale, origin, color, angle, depth);
        }

        public static void DrawLine(Vector2 from, Vector2 to, Color color, float depth, int width = 1)
        {
            float angle = Maths.Atan2(from, to);
            float length = from.Subtract(to).GetMagnitude();

            Draw(s_blankTexture, from, new Vector2(length, width), new Vector2(0f, 0.5f), color, angle, depth);
        }

        public static void BeginLayer(Layer layer)
        {
            if (s_currentLayer is not null)
                throw new InvalidOperationException("Current layer must be ended before starting drawing on a new one");
            else if (s_drawnLayers.Contains(layer))
                throw new InvalidOperationException("This layer has already been drawn on this frame");

            s_currentLayer = layer;
            s_drawnLayers.Add(layer);

            Rectangle layerScissor = layer.Scissor;

            int layerHeight = layer.Height;
            int scissorY = (int)layerScissor.Y;
            int scissorHeight = (int)layerScissor.Height;

            if (scissorY != 0)
                scissorY = layerHeight - scissorY - scissorHeight;

            s_spriteBatch.Begin(layer.RenderTarget, layer.ClearColor, new Rectangle(layerScissor.X, scissorY, layerScissor.Width, layerScissor.Height), layer.Camera.GetViewMatrix());
        }

        public static void EndCurrentLayer()
        {
            if (s_currentLayer is null)
                throw new InvalidOperationException("No current layer");

            s_currentLayer = null;

            s_spriteBatch.End();
        }

        public static void DrawLayers()
        {
            if (s_currentLayer is not null)
                throw new InvalidOperationException("Current layer must be ended before before drawing all layers");

            int resolutionWidth = Resolution.CurrentWidth;
            int resolutionHeight = Resolution.CurrentHeight;

            Matrix4 identity = Matrix4.Identity;

            s_spriteBatch.Begin(s_finalTarget, ClearColor, Scissor, identity);

            foreach (Layer layer in s_drawnLayers)
            {
                Camera camera = layer.Camera;

                float zoom = camera.Zoom;

                int width = layer.Width;
                int height = layer.Height;

                float newWidth = resolutionWidth * zoom;
                float newHeight = resolutionHeight * zoom;

                float x = (resolutionWidth - newWidth) / 2f;
                float y = (resolutionHeight - newHeight) / 2f;

                s_spriteBatch.Submit(layer.RenderTarget.Texture, layer.PostProcessingEffect, new Rectangle(0f, 0f, width, height), new Vector2(x, y), Resolution.CurrentSize / new Vector2(width, height) * zoom, Vector2.Zero, Color.White, 0f, SpriteEffect.None, SpriteEffect.None, layer.Depth);

                layer.ResetScissor();
            }

            s_spriteBatch.End();
            s_drawnLayers.Clear();

            ResetScissor();

            s_spriteBatch.Begin(s_screenTarget, ClearColor, Scissor, identity);
            s_spriteBatch.Submit(s_finalTarget.Texture, PostProcessingEffect, new Rectangle(0f, 0f, s_finalTarget.Width, s_finalTarget.Height), Vector2.Zero, new Vector2(1f), Vector2.Zero, Color.White, 0f, SpriteEffect.None, SpriteEffect.None, 1f);

            s_spriteBatch.End();
        }

        public static void ResetScissor()
        {
            Scissor = new Rectangle(0f, 0f, Resolution.CurrentWidth, Resolution.CurrentHeight);
        }

        private static void OnResolutionChnaged(object sender, EventArgs args)
        {
            ApplyResolution();
        }

        private static void ApplyResolution()
        {
            // TODO: Delete the previous render target texture if it exists (WHEN THE TEXTURE CLASS WILL HAVE THIS OPTION)!!!!!!!

            int width = Resolution.CurrentWidth;
            int height = Resolution.CurrentHeight;

            Texture texture = Texture.CreateEmpty(width, height);

            s_finalTarget = RenderTarget.FromTexture(texture);
            s_screenTarget = RenderTarget.FromScreen(width, height);
        }
    }
}
