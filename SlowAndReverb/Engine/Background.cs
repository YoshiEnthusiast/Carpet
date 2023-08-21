using System.Collections.Generic;

namespace SlowAndReverb
{
    public abstract class Background
    {
        public const float MinDepth = 0f;
        public const float MaxDepth = 1f;

        private readonly Sprite _mainImage;
        private readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();

        public Background(string mainImageName)
        {
            _mainImage = CreateSprite(mainImageName);
            _mainImage.Depth = MinDepth;
        }

        public float ScrollScale { get; set; } = 0.001f;

        public virtual void Update(float deltaTime)
        {
            Layer foregroundLayer = Layers.Foreground;
            Vector2 cameraPosition = foregroundLayer.Camera.Position;

            foreach (ParallaxLayer layer in _layers)
            {
                RepeatTextureMaterial material = layer.Material;
                float depth = 1f - layer.Depth;

                material.Scroll = new Vector2(cameraPosition.X / depth * ScrollScale, 0f);
            }
        }

        public virtual void Draw()
        {
            _mainImage.Draw(Vector2.Zero);

            foreach (ParallaxLayer layer in _layers)
            {
                Sprite sprite = layer.Sprite;
                sprite.Draw(Vector2.Zero);
            }
        }

        public void PushParallaxLayer(string spriteName, float depth)
        {
            depth = Maths.Clamp(depth, MinDepth, MaxDepth);

            var material = new RepeatTextureMaterial();
            var sprite = CreateSprite(spriteName);

            sprite.Material = material;
            sprite.Depth = depth;

            var layer = new ParallaxLayer(sprite, material, depth);

            _layers.Add(layer);
        } 

        private Sprite CreateSprite(string name)
        {
            var sprite = new Sprite(name);

            Layer backgroundLayer = Layers.Background;
            SmoothCameraLayer foregroundLayer = Layers.Foreground;

            float scaleX = backgroundLayer.Width / foregroundLayer.VisibleWidth;
            float scaleY = backgroundLayer.Height / foregroundLayer.VisibleHeight;

            sprite.Scale = new Vector2(scaleX, scaleY);
            sprite.Origin = Vector2.Zero;

            return sprite;
        }

        private readonly record struct ParallaxLayer(Sprite Sprite, RepeatTextureMaterial Material, float Depth);
    }
}
