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
        private static readonly Renderer s_renderer = new Renderer(true);

        private static Texture s_finalTarget = Texture.CreateEmpty(1280, 720);
        private static Layer s_currentLayer;

        static Graphics()
        {
            Resolution.Change += OnResolutionChnaged;
        }

        public static Layer CurrentLayer => s_currentLayer;

        public static Material PostProcessingEffect { get; set; }
        public static Color ClearColor { get; set; }
        public static Rectangle Scissor { get; set; }

        public static void Draw(Texture texture, Vector2 position, Vector2 scale, Vector2 origin, float angle, float alpha, bool flipHorizontal, bool flipVertical, float depth)
        {
            Draw(texture, new Rectangle(0f, 0f, texture.Width, texture.Height), position, scale, origin, angle, alpha, flipHorizontal, flipVertical, depth);
        }

        public static void Draw(Texture texture, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, float angle, float alpha, bool flipHorizontal, bool flipVertical, float depth)
        {
            s_renderer.Submit(texture, bounds, position, scale, origin, angle, alpha, flipHorizontal, flipVertical, depth);
        }

        public static void Draw(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, float angle, float alpha, bool flipHorizontal, bool flipVertical, float depth)
        {
            s_renderer.Submit(texture, material, bounds, position, scale, origin, angle, alpha, flipHorizontal, flipVertical, depth);
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

            s_renderer.Begin(layer.RenderTarget, layer.ClearColor, layer.Width, layerHeight, new Rectangle(layerScissor.X, scissorY, layerScissor.Width, layerScissor.Height), layer.Camera.GetViewMatrix());
        }

        public static void EndCurrentLayer()
        {
            if (s_currentLayer is null)
                throw new InvalidOperationException("No current layer");

            s_currentLayer = null;

            s_renderer.FlushDrawCalls();
        }

        public static void DrawLayers()
        {
            if (s_currentLayer is not null)
                throw new InvalidOperationException("Current layer must be ended before before drawing all layers");

            Vector2 resolutionSize = Resolution.Current.Size;

            int resolutionWidth = resolutionSize.RoundedX;
            int resolutionHeight = resolutionSize.RoundedY;

            Matrix4 identity = Matrix4.Identity;

            s_renderer.Begin(s_finalTarget, ClearColor, resolutionWidth, resolutionHeight, Scissor, identity);

            // TODO: Убрать эту грязь
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

                s_renderer.Submit(layer.RenderTarget, layer.PostProcessingEffect, new Rectangle(0f, 0f, width, height), new Vector2(x, y), resolutionSize / new Vector2(width, height) * zoom, Vector2.Zero, 0f, 1f, false, false, layer.Depth);

                layer.ResetScissor();
            }

            s_renderer.FlushDrawCalls();
            s_drawnLayers.Clear();

            ResetScissor();

            s_renderer.Begin(null, ClearColor, resolutionWidth, resolutionHeight, Scissor, identity);
            s_renderer.Submit(s_finalTarget, PostProcessingEffect, new Rectangle(0f, 0f, s_finalTarget.Width, s_finalTarget.Height), Vector2.Zero, new Vector2(1f), Vector2.Zero, 0f, 1f, false, false, 1f);

            //s_finalTarget.SaveAsPng("xd.png");

            s_renderer.FlushDrawCalls();
        }

        public static void ResetScissor()
        {
            Resolution currentResolution = Resolution.Current;

            Scissor = new Rectangle(0f, 0f, currentResolution.Width, currentResolution.Height);
        }

        private static void OnResolutionChnaged(object sender, EventArgs args)
        {
            // Как-то поменять разрешение s_finalTarget
        }
    }
}
