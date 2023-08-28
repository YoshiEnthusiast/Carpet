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

        private readonly VirtualTexture _grapleHookTexture = Content.GetVirtualTexture("grapleHook");
        private readonly Light[] _lights;

        private readonly Sprite s;

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

            _lights = new Light[]
            {
                Add(new Light()
                {
                    Color = Color.Red
                }),

                Add(new Light()
                {
                    Color = new Color(255, 115, 3)
                }),

                Add(new Light()
                {
                    Color = new Color(255, 211, 3)
                }),

                Add(new Light()
                {
                    Color = new Color(73, 252, 3)
                }),

                Add(new Light()
                {
                    Color = new Color(3, 7, 252)
                }),

                Add(new Light()
                {
                    Color = new Color(251, 0, 255)
                }),
            };

            int length = _lights.Length;

            for (int i = 0; i < length; i++)
            {
                Light light = _lights[i];

                light.Rotation = Maths.TwoPI / length * i;

                light.Radius = 60f;
                light.Angle = Maths.PI / 3f;
                light.FalloffAngle = 0.4f;
                light.StartDistance = 0.2f;
                light.StartFade = 0.1f;
                light.Volume = 0.8f;
            }

            //Add(new Light()
            //{
            //    Color = Color.Yellow,
            //    Radius = 60f,
            //    Angle = Maths.PI / 3f,
            //    FalloffAngle = 0.4f,
            //    StartDistance = 0.2f,
            //    StartFade = 0.1f,
            //    Volume = 0.8f
            //});

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

            //s = Add(new Sprite("uiArrow"));
        }

        protected override void Update(float deltaTime)
        {
            Position = Layers.Foreground.MousePosition;
            //Get<Light>().Rotation += 0.01f;

            foreach (Light light in _lights)
                light.Rotation += 0.01f;

            //_emitter.Awake = Input.IsMouseDown(MouseButton.Left);

            if (Input.IsMousePressed(MouseButton.Right))
            {
                if (_emitter.Behaviour.Follow is null)
                    _emitter.Behaviour.Follow = this;
                else
                    _emitter.Behaviour.Follow = null;
            }
        }

        private readonly RepeatTextureMaterial _material = new RepeatTextureMaterial()
        {
            Scale = new Vector2(24f, 1f)
        };
        float angle = 0f;

        protected override void Draw()
        {
            //Graphics.Draw(_grapleHookTexture, _material, Layers.Foreground.MousePosition, new Vector2(24f, 1f), new Vector2(0f, 2f), Color.White, angle, Depths.Debug);

            //Graphics.Draw(_grapleHookTexture, Layers.Foreground.MousePosition, new Vector2(50f, 1f), Vector2.Zero, Color.White, angle, Depths.Debug);
            angle -= 0.005f;

            base.Draw();
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
