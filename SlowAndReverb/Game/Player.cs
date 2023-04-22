using System;
using System.Collections;

namespace SlowAndReverb
{
    public class Player : PhysicsBody
    {
        #region Constants

        private const float Acceleration = 0.3f;
        private const float AirAcceleration = 0.2f;
        private const float MaxHorizontalVelocity = 3f;
        private const float Friction = 0.2f;
        private const float AirFriction = 0.1f;
        private const float Weight = 0.2f;
        private const float InitialThrust = 1.5f;
        private const float Thrust = 0.6f;
        private const float JumpTime = 5f;

        private const float MaxGrapplingAngle = Maths.HalfPI;
        private const float GrapplingAngleSpeed = -0.05f;

        private const float AimDisplayDistance = 30f;
        private const int AimDispayLinesCount = 6;
        private const float AimDisplayLineLength = 10f;
        private const float AimDisplayAlpha = 0.6f;
        private const float AimCrosshairDistance = 27f;

        #endregion

        private readonly StateMachine<State> _stateMachine;
        private readonly CoroutineRunner _coroutineRunner;
        private readonly Sprite _sprite;

        private readonly Sprite _hookRopeSprite;
        private readonly Sprite _hookGrapleSprite = new Sprite("hookGraple");
        private readonly RepeatTextureMaterial _hookRopeMaterial = new RepeatTextureMaterial();

        private float _jumpTimer;

        private float _grapplingAngle;
        private float _initialGrapplingAngle;
        private float _grapplingAngleDestination;
        private sbyte _grapplingDirection;

        private bool _drawGrapple;

        public Player(float x, float y) : base(x, y)  
        {
            Size = new Vector2(16f, 24f);

            _stateMachine = Add(new StateMachine<State>());

            _stateMachine.SetState(State.Regular, UpdateRegular, null);
            _stateMachine.SetState(State.Aiming, UpdateAim, DrawAim, StartAiming, null);

            _stateMachine.ForceState(State.Regular);

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

        public sbyte Direction { get; set; } = 1;

        protected override void Update(float deltaTime)
        {
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

        private void UpdateMovement(float deltaTime)
        {
            float velocityX = VelocityX;
            float xAxis = Input.XAxis.GetValue();

            if (xAxis != 0f)
            {
                float acceleration = (Grounded ? Acceleration : AirAcceleration) * deltaTime;

                Direction = (sbyte)Input.XAxis.GetSign();
                VelocityX = Maths.ApproachAbs(velocityX, acceleration * xAxis, MaxHorizontalVelocity);

                _sprite.SetAnimation(null);
            }
            else
            {
                float friction = (Grounded ? Friction : AirFriction) * deltaTime;

                if (velocityX > 0f)
                    VelocityX = Math.Max(velocityX - friction, 0f);
                else
                    VelocityX = Math.Min(velocityX + friction, 0f);

                if (velocityX == 0f)
                    _sprite.SetAnimation("idle");
            }

            if (Grounded)
            {
                if (Input.Jump.IsPressed())
                {
                    VelocityY -= InitialThrust * deltaTime;

                    _coroutineRunner.StartCoroutine(CreateJumpCoroutine());
                }
            }
            else
            {
                VelocityY += Weight;
            }
        }

        private void UpdateRegular(float deltaTime)
        {
            UpdateMovement(deltaTime);

            if (Input.Grapple.IsPressed())
                _stateMachine.ForceState(State.Aiming);
        }

        private void StartAiming()
        {
            _grapplingDirection = Direction;
            _grapplingAngleDestination = MaxGrapplingAngle;

            if (Direction >= 0)
            {
                _grapplingAngle = 0f;
            }
            else
            {
                float pi = Maths.PI;

                _grapplingAngle = pi;
                _grapplingAngleDestination += pi;
            }

            _initialGrapplingAngle = _grapplingAngle;            
        }

        private void UpdateAim(float deltaTime)
        {
            UpdateMovement(deltaTime);

            if (Input.Grapple.IsDown())
            {
                float deltaAngle = GrapplingAngleSpeed * deltaTime * _grapplingDirection;

                _grapplingAngle = Maths.ApproachAbs(_grapplingAngle, deltaAngle, _grapplingAngleDestination);
            }
            else
            {
                _stateMachine.ForceState(State.Regular);
            }
        }

        private void DrawAim()
        {
            float depth = Depths.WorldUI;
            Color red = Color.Red;

            float deltaAngle = Maths.DeltaAngle(_initialGrapplingAngle, _grapplingAngleDestination) / (AimDispayLinesCount - 1);
            deltaAngle *= -_grapplingDirection;

            for (int i = 0; i < AimDispayLinesCount; i++)
            {
                float angle = _initialGrapplingAngle + deltaAngle * i;

                Vector2 linePosition = Rotate(AimDisplayDistance, angle);
                Color color = Color.Lerp(Color.Yellow, red, i / (float)AimDispayLinesCount);

                Graphics.DrawLine(linePosition, angle, AimDisplayLineLength, color * AimDisplayAlpha, depth);
            }

            Vector2 aimDisplayPosition = Rotate(AimCrosshairDistance, _grapplingAngle);
            Graphics.FillRectangle(Rectangle.FromCenter(aimDisplayPosition, new Vector2(2f)), red * AimDisplayAlpha, depth);
        }

        private IEnumerator CreateJumpCoroutine()
        {
            _jumpTimer = JumpTime;

            while (true)
            {
                if (!Input.Jump.IsDown() || _jumpTimer <= 0f)
                    yield break;

                float time = Engine.DeltaTime;

                VelocityY -= Thrust * time;
                _jumpTimer -= time;

                yield return null;
            }
        }

        private enum State
        {
            Regular,
            Aiming,
            Grappling
        }
    }
}
