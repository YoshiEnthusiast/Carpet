using System;
using System.Collections;

namespace SlowAndReverb
{
    public class Player : Entity
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

        private readonly PhysicsBody _physicsBody;
        private readonly CoroutineRunner _coroutineRunner;
        private readonly Sprite _sprite;

        private readonly Sprite _hookRopeSprite;
        private readonly Sprite _hookGrapleSprite = new Sprite("hookGraple");
        private readonly RepeatTextureMaterial _hookRopeMaterial = new RepeatTextureMaterial();

        private float _jumpTimer;
        private bool _drawGrapple;

        public Player(float x, float y) : base(x, y)  
        {
            Size = new Vector2(16f, 24f);

            _physicsBody = Add(new PhysicsBody());
            _coroutineRunner = Add(new CoroutineRunner());

            _sprite = Add(new Sprite("player", (int)Width, (int)Height)
            {
                Depth = Depths.Player
            });

            _sprite.AddAnimation("idle", new Animation(40f, true, new int[]
            {
                0,
                1
            }));

            _hookRopeSprite = new Sprite("grapleHook")
            {
                Material = _hookRopeMaterial
            };

            _hookRopeSprite.Origin = new Vector2(0f, _hookRopeSprite.Height / 2f);
        }

        public sbyte Direction { get; set; }

        protected override void Update(float deltaTime)
        {
            bool grounded = _physicsBody.Grounded;

            float velocityX = _physicsBody.VelocityX;
            float velocityY = _physicsBody.VelocityY;

            float acceleration = (grounded ? Acceleration : AirAcceleration) * deltaTime;

            if (Input.IsDown(Key.D))
            {
                _physicsBody.VelocityX = Math.Min(velocityX + acceleration, MaxHorizontalVelocity);

                _sprite.SetAnimation(null);
            }
            else if (Input.IsDown(Key.A))
            {
                _physicsBody.VelocityX = Math.Max(velocityX - acceleration, -MaxHorizontalVelocity);

                _sprite.SetAnimation(null);
            }
            else
            {
                float friction = (grounded ? Friction : AirFriction) * deltaTime;

                if (velocityX > 0f)
                    _physicsBody.VelocityX = Math.Max(velocityX - friction, 0f);
                else
                    _physicsBody.VelocityX = Math.Min(velocityX + friction, 0f);

                if (velocityX == 0f)
                    _sprite.SetAnimation("idle");
            }

            if (grounded)
            {
                if (Input.IsPressed(Key.Space))
                {
                    _physicsBody.VelocityY -= InitialThrust * deltaTime;

                    _coroutineRunner.StartCoroutine(CreateJumpCoroutine());
                }
            }
            else
            {
                _physicsBody.VelocityY += Weight;
            }

            // hook sprite update...
            Vector2 destination = Layers.Foreground.MousePosition;
            float distance = Vector2.Distance(Position, destination);

            var scale = new Vector2(distance / _hookRopeSprite.Texture.Width, 1f);
            float angle = Maths.Atan2(Position, destination); 

            _hookRopeSprite.Scale = scale;
            _hookRopeSprite.Angle = angle;

            _hookGrapleSprite.OverridenPosition = destination;
            _hookGrapleSprite.Angle = angle;

            _hookRopeMaterial.Scale = scale;


            if (Input.IsPressed(Key.O))
                _drawGrapple = !_drawGrapple;

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            if (_drawGrapple)
            {
                _hookRopeSprite.Draw(Position);
                _hookGrapleSprite.DrawOnDefaultPosition();
            }
        }

        private IEnumerator CreateJumpCoroutine()
        {
            _jumpTimer = JumpTime;

            while (true)
            {
                if (!Input.IsDown(Key.Space) || _jumpTimer <= 0f)
                    yield break;

                float time = Engine.DeltaTime;

                _physicsBody.VelocityY -= Thrust * time;
                _jumpTimer -= time;

                yield return null;
            }
        }
    }
}
