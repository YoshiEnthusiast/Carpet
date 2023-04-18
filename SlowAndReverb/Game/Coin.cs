using System;
using System.Collections;

namespace SlowAndReverb
{
    public class Coin : Entity
    {
        private readonly Sprite _sprite;

        private readonly RectangleParticleEmitter _particleEmitter;
        private readonly CircleParticleEmitter _collectedParticleEmitter;

        private readonly Light _light;

        private bool _collected;

        public Coin(float x, float y) : base(x, y)
        {
            Size = new Vector2(10f, 10f);

            _light = Add(new Light()
            {
                Radius = 15f
            });

            _sprite = Add(new Sprite("coin", Size.RoundedX, Size.RoundedY)
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

            var behaviour = new ParticleBehaviour()
            {
                Velocity = new Vector2(0f, -0.15f),
                VelocityVariation = new Vector2(0f, 0.01f),
                StartingColor = Color.Yellow,
                DestinationColor = Color.White,
                LifeDuration = 50f
            };

            _particleEmitter = Add(new RectangleParticleEmitter(behaviour, Width + 2f, Height / 2f)
            {
                PositionOffset = new Vector2(0f, -Height / 4f),
                Interval = 20f,
                IntervalVariation = 1f
            });

            var offCenterBehavoiur = new OffCenterParticleBehaviour()
            {
                Velocity = new Vector2(0.3f, 0f),
                VelocityVariation = new Vector2(0.1f, 0f),
                StartingColor = Color.Yellow,
                DestinationColor = Color.Lerp(Color.Yellow, Color.White, 0.8f),
                Follow = this,
                LifeDuration = 30f
            };

            _collectedParticleEmitter = Add(new CircleParticleEmitter(offCenterBehavoiur, HalfWidth / 4f)
            {
                EmitCount = 8,
                EmitCountVariation = 2,
                ShapeBehaviour = ParticleShapeBehaviour.EmitAroundBorder,
                Awake = false
            });
        }

        protected override void Update(float deltaTime)
        {
            if (_collected)
                return;

            if (Scene.CheckRectangle<Player>(Rectangle) is not null)
            {
                _collectedParticleEmitter.Emit();
                _particleEmitter.Awake = false;

                _sprite.DelayMultiplier = 0.1f;

                Scene.StartCoroutine(UpdateFlyAway());

                _collected = true;
            }
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
