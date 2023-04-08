using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TestEntity : Entity
    {
        private readonly Font _font = new Font("testFont");
        private readonly CircleParticleEmitter _emitter;

        public TestEntity(float x, float y) : base(x, y)
        {
            var sprite = new Sprite("needler", 19, 16)
            {
                Depth = 1f,
            };

            sprite.AddAnimation("amogus", new Animation(1f, true, new int[]
            {
                4,
                3,
                2,
                1,
                0
            }));

            sprite.SetAnimation("amogus");

            //Add(sprite);

            Add(new Light()
            {
                Color = new Color(180, 180, 180),
                Radius = 60f
            });

            var behaviour = new OffCenterParticleBehaviour()
            {
                Velocity = new Vector2(0.4f, 0f),
                VelocityVariation = new Vector2(0.1f, 0f),
                StartingColor = Color.Blue,
                DestinationColor = Color.Lerp(Color.Blue, Color.White, 0.8f),
                LifeDuration = 70f,
                DestinationScale = new Vector2(4f),
                Follow = this
            };

            _emitter = Add(new CircleParticleEmitter(behaviour, 10f)
            {
                Awake = false,
                EmitCount = 5,
                EmitCountVariation = 2
            });
        }

        protected override void Update(float deltaTime)
        {
            Position = Layers.Foreground.MousePosition;

            _emitter.Awake = Input.IsMouseDown(MouseButton.Left);

            if (Input.IsMousePressed(MouseButton.Right))
            {
                if (_emitter.Behaviour.Follow is null)
                    _emitter.Behaviour.Follow = this;
                else
                    _emitter.Behaviour.Follow = null;
            }
        }

        protected override void Draw()
        {

        }

        private class OffCenterParticleBehaviour : ParticleBehaviour
        {
            public override ParticleData Create(Vector2 position, Vector2 sourcePosition)
            {
                ParticleData data = base.Create(position, sourcePosition);
                float angle = Maths.Atan2(position, sourcePosition);

                data.Velocity = -data.Velocity.Rotate(angle);

                return data;
            }
        }
    }
}
