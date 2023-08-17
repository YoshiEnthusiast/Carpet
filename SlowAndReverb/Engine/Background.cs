using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public abstract class Background
    {
        private readonly Sprite _mainImage;
        private readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();

        public Background(string spriteName)
        {
            _mainImage = new Sprite(spriteName);
        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void Draw(Scene scene)
        {
            _mainImage.Draw(Vector2.Zero);

            foreach (ParallaxLayer layer in _layers)
            {
                Sprite sprite = layer.sprite;
                RepeatTextureMaterial material = sprite.Material as RepeatTextureMaterial;
                float distance = layer.Distance;

                CameraSystem cameraSystem = scene.GetSystem<CameraSystem>();
                float xScroll = cameraSystem.CameraPosition.X * (1f - distance);

                material.Scroll = new Vector2(xScroll, 0f);
            }
        }

        protected void AddLayer(string spriteName, float distance)
        {
            distance = Math.Clamp(distance, 0f, 1f);

            var material = new RepeatTextureMaterial();

            var sprite = new Sprite(spriteName)
            {
                Material = material,
                Depth = distance
            };

            Vector2 layerSize = Layers.Background.Size;
            sprite.Scale = layerSize / sprite.Size;

            var parallaxLayer = new ParallaxLayer(sprite, distance);

            _layers.Add(parallaxLayer);
        }

        private readonly record struct ParallaxLayer(Sprite sprite, float Distance);
    }
}
