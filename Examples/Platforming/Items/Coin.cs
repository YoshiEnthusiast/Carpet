using System;
using System.Collections;

namespace Carpet.Platforming
{
    public class Coin : Entity
    {
        private readonly Light _light;

        private RectangleParticleEmitter _particleEmitter;
        private CircleParticleEmitter _collectedParticleEmitter;
        private Sprite _sprite;

        private bool _collected;

        public Coin(float x, float y) : base(x, y)
        {
            Size = new Vector2(10f, 10f);

            _light = Add(new Light()
            {
                Radius = 15f
            });
        }

        protected string SpriteName { get; init; } = "coin";
        protected Color Color { get; init; } = Color.Yellow;

        protected override void Update(float deltaTime)
        {
            if (Scene.CheckRectangle<Player>(Rectangle) is not null)
                Collect();
        }

        protected override void OnAdded()
        {
            var behaviour = new ParticleBehavior()
            {
                Velocity = new Vector2(0f, -0.15f),
                VelocityVariation = new Vector2(0f, 0.01f),
                StartingColor = Color,
                DestinationColor = Color.White,
                LifeDuration = 50f
            };

            _particleEmitter = Add(new RectangleParticleEmitter(behaviour, Width + 2f, Height / 2f)
            {
                PositionOffset = new Vector2(0f, -Height / 4f),
                Interval = 20f,
                IntervalVariation = 1f
            });

            var offCenterBehavoiur = new OffCenterParticleBehavior()
            {
                Velocity = new Vector2(0.3f, 0f),
                VelocityVariation = new Vector2(0.1f, 0f),
                StartingColor = Color,
                DestinationColor = Color.Lerp(Color, Color.White, 0.8f),
                Follow = this,
                LifeDuration = 30f
            };

            _collectedParticleEmitter = Add(new CircleParticleEmitter(offCenterBehavoiur, HalfWidth / 4f)
            {
                EmitCount = 8,
                EmitCountVariation = 2,
                ShapeBehavior = ParticleShapeBehavior.EmitAroundBorder,
                Awake = false
            });

            _sprite = Add(new Sprite(SpriteName, Size.FlooredX, Size.FlooredY)
            {
                Depth = Depths.Items
            });

            _sprite.AddAnimation("spin", new Animation(10f, true, new int[]
            {
                0,
                1,
                2,
                3,
                2,
                1
            }));

            _sprite.SetAnimation("spin");
        }

        protected void Collect()
        {
            if (_collected)
                return;

            _collectedParticleEmitter.Emit();
            _particleEmitter.Awake = false;

            _sprite.DelayMultiplier = 0.1f;

            Scene.StartCoroutine(UpdateFlyAway());

            _collected = true;
        }

        private IEnumerator UpdateFlyAway()
        {
            for (int i = 0; i < 7; i++)
            {
                Y -= 1f;

                _light.Color *= 0.8f;
                _sprite.Color *= 0.8f;

                yield return 4f;
            }

            Scene.Remove(this);
        }
    }
}
