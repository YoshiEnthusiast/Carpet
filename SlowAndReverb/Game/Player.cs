using System;
using System.Collections;

namespace SlowAndReverb
{
    public class Player : PhysicsObject
    {
        private const float Acceleration = 0.3f;
        private const float AirAcceleration = 0.2f;
        private const float MaxHorizontalVelocity = 3f;
        private const float Friction = 0.2f;
        private const float AirFriction = 0.1f;
        private const float Weight = 0.2f;
        private const float InitialThrust = 1.5f;
        private const float Thrust = 0.6f;
        private const float JumpTime = 5f;

        private readonly CoroutineRunner _coroutineRunner;
        private readonly Sprite _sprite;

        private float _jumpTimer;

        public Player(float x, float y) : base(x, y)  
        {
            Size = new Vector2(16f, 24f);

            _coroutineRunner = Add(new CoroutineRunner());
            _sprite = Add(new Sprite("player", (int)Width, (int)Height));

            _sprite.AddAnimation("idle", new Animation(40f, true, new int[]
            {
                0,
                1
            }));
        }

        public sbyte Direction { get; set; }

        protected override void Update(float deltaTime)
        {
            float acceleration = (Grounded ? Acceleration : AirAcceleration) * deltaTime;

            if (Input.IsDown(Key.D))
            {
                VelocityX = Math.Min(VelocityX + acceleration, MaxHorizontalVelocity);

                _sprite.SetAnimation(null);
            }
            else if (Input.IsDown(Key.A))
            {
                VelocityX = Math.Max(VelocityX - acceleration, -MaxHorizontalVelocity);

                _sprite.SetAnimation(null);
            }
            else
            {
                float friction = (Grounded ? Friction : AirFriction) * deltaTime;

                if (VelocityX > 0f)
                    VelocityX = Math.Max(VelocityX - friction, 0f);
                else
                    VelocityX = Math.Min(VelocityX + friction, 0f);

                if (VelocityX == 0f)
                    _sprite.SetAnimation("idle");
            }

            if (Grounded)
            {
                if (Input.IsPressed(Key.Space))
                {
                    VelocityY -= InitialThrust * deltaTime;

                    _coroutineRunner.StartCoroutine(CreateJumpCoroutine());
                }
            }
            else
            {
                VelocityY += Weight;
            }

            base.Update(deltaTime);
        }

        private IEnumerator CreateJumpCoroutine()
        {
            _jumpTimer = JumpTime;

            while (true)
            {
                if (!Input.IsDown(Key.Space) || _jumpTimer <= 0f)
                    yield break;

                float time = Engine.DeltaTime;

                VelocityY -= Thrust * time;
                _jumpTimer -= time;

                yield return null;
            }
        }
    }
}
