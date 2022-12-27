using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public static class Graphics
    {
        private static readonly List<Layer> s_drawnLayers = new List<Layer>();
        private static readonly SpriteBatch s_spriteBatch = new SpriteBatch(true);

        private static readonly List<CircleMaterial> s_circleMaterials = new List<CircleMaterial>();

        private static VirtualTexture s_blankTexture;

        private static RenderTarget s_finalTarget;
        private static RenderTarget s_screenTarget;

        private static Layer s_currentLayer;
        
        public static SpriteBatch SpriteBatch => s_spriteBatch;
        public static Layer CurrentLayer => s_currentLayer;

        public static Material PostProcessingEffect { get; set; }
        public static Color ClearColor { get; set; }
        public static Rectangle Scissor { get; set; }

        internal static void Initialize()
        {
            Resolution.Change += OnResolutionChnaged;
            ApplyResolution();

            s_blankTexture = Content.GetVirtualTexture("blank");

            Material.InitializeUniforms();
        }

        public static void Draw(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            DrawWithoutRoundingPosition(texture, material, bounds, position.Round(), scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
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
            Draw(texture, material, position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public static void Draw(VirtualTexture virtualTexture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Draw(virtualTexture.ActualTexture, material, virtualTexture.GetBounds(bounds), position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        public static void Draw(VirtualTexture virtualTexture, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Draw(virtualTexture, null, bounds, position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        public static void Draw(VirtualTexture virtualTexture, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Draw(virtualTexture, bounds, position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public static void Draw(VirtualTexture virtualTexture, Material material, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Draw(virtualTexture, material, new Rectangle(Vector2.Zero, new Vector2(virtualTexture.Width, virtualTexture.Height)), position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        public static void Draw(VirtualTexture virtualTexture, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Draw(virtualTexture, new Rectangle(Vector2.Zero, new Vector2(virtualTexture.Width, virtualTexture.Height)), position, scale, origin, color, angle, depth);
        }

        public static void Draw(VirtualTexture virtualTexture, Material material, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Draw(virtualTexture, material, position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public static void DrawLine(Vector2 from, Vector2 to, Color color, float depth, int width = 1, bool centred = true)
        {
            float angle = Maths.Atan2(from, to);
            float length = from.Subtract(to).GetMagnitude();

            float originY = centred ? 0.5f : 0f;

            Draw(s_blankTexture, from, new Vector2(length, width), new Vector2(0f, originY), color, angle, depth);
        }

        public static void DrawRectangle(Rectangle rectangle, Color color, float depth, int lineWidth = 1, bool centred = false)
        {
            Vector2 topLeft = rectangle.TopLeft;
            Vector2 topRight = rectangle.TopRight;
            Vector2 bottomLeft = rectangle.BottomLeft;
            Vector2 bottomRight = rectangle.BottomRight;

            DrawLine(topLeft, topRight, color, depth, lineWidth, centred);
            DrawLine(topRight, bottomRight, color, depth, lineWidth, centred);
            DrawLine(bottomRight, bottomLeft, color, depth, lineWidth, centred);
            DrawLine(bottomLeft, topLeft, color, depth, lineWidth, centred);
        }

        public static void DrawRectangle(Vector2 topLeft, Vector2 bottomRight, Color color, float depth, int lineWidth = 1, bool centred = false)
        {
            DrawRectangle(new Rectangle(topLeft, bottomRight), color, depth, lineWidth, centred);
        }

        public static void FillRectabgle(Rectangle rectangle, Color color, float depth)
        {
            Draw(s_blankTexture, rectangle.TopLeft, rectangle.Size, Vector2.Zero, color, 0f, depth);
        }

        public static void FillRectabgle(Vector2 topLeft, Vector2 bottomRight, Color color, float depth)
        {
            FillRectabgle(new Rectangle(topLeft, bottomRight), color, depth);
        }

        public static void DrawCircle(Vector2 position, Color color, int radius, float depth, float lineWidth = 1f)
        {
            float circumference = radius * 2f;
            CircleMaterial material = GetCircleMaterial(lineWidth, circumference);

            Draw(s_blankTexture, material, position, new Vector2(circumference), new Vector2(0.5f), color, 0f, depth);
        }

        public static void FillCircle(Vector2 position, Color color, int raduis, float depth)
        {
            DrawCircle(position, color, raduis, depth, raduis);
        }

        public static void DrawCircleWithLines(Vector2 position, Color color, int raduis, float depth, int lineWidth = 1, bool centred = true, int linesCount = 18)
        {
            Vector2 startPosition = position.AddX(raduis);
            Vector2 previousPosition = startPosition;
            float deltaAngle = Maths.TwoPI / linesCount;

            for (int i = 1; i < linesCount + 1; i++)
            {
                float angle = deltaAngle * i;
                Vector2 currentPosition = startPosition.Rotate(position, angle);

                DrawLine(previousPosition, currentPosition, color, depth, lineWidth, centred);

                previousPosition = currentPosition;
            }
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
            int scissorY = (int)layerScissor.Top;
            int scissorHeight = (int)layerScissor.Height;

            if (scissorY != 0)
                scissorY = layerHeight - scissorY - scissorHeight;

            s_spriteBatch.Begin(layer.RenderTarget, layer.ClearColor, new Rectangle(layerScissor.Left, scissorY, layerScissor.Width, layerScissor.Height), layer.Camera.GetViewMatrix());
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

        internal static void DrawWithoutRoundingPosition(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            s_spriteBatch.Submit(texture, material, bounds, position, scale, origin, color, angle, horizontalEffect, verticalEffect, depth);
        }

        internal static void DrawWithoutRoundingPosition(VirtualTexture virtualTexture, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            DrawWithoutRoundingPosition(virtualTexture.ActualTexture, null, virtualTexture.GetBounds(new Rectangle(Vector2.Zero, new Vector2(virtualTexture.Width, virtualTexture.Height))), position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth); ;
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

        private static CircleMaterial GetCircleMaterial(float lineWidth, float circumference)
        {
            foreach (CircleMaterial material in s_circleMaterials)
                if (material.LineWidth == lineWidth && material.Circumference == circumference)
                    return material;

            var newMaterial = new CircleMaterial()
            {
                LineWidth = lineWidth,
                Circumference = circumference
            };

            s_circleMaterials.Add(newMaterial);

            return newMaterial;
        }
    }
}
