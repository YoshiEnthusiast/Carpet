using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Coin : Entity
    {
        private readonly Sprite _sprite;

        public Coin(float x, float y) : base(x, y)
        {
            Size = new Vector2(10f, 10f);

            Add(new Light()
            {
                Radius = 15f
            });

            _sprite = Add(new Sprite("coin", Size.RoundedX, Size.RoundedY));

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
                LifeDuration = 50f,
                Depth = 10f,
            };

            Add(new RectangleParticleEmitter(behaviour, Width + 2f, Height / 2f)
            {
                PositionOffset = new Vector2(0f, -Height / 4f),
                Interval = 20f,
                IntervalVariation = 1f
            });
        }
    }
}
