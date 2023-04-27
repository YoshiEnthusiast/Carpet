using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        private const float GrappleLength = 140f;
        private const float MaxGrappleAngle = Maths.HalfPI;
        private const float GrappleAngleSpeed = -0.05f;
        private const float GrappleAngleDeadzone = 0.3f;
        private const float GrappleStartDistance = 8f;
        private const float GrappleShootingLerpSpeed = 0.5f;
        private const float GrapplingLerpSpeed = 3f;

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

        private Anchor _anchor;
        private Vector2 _grappleDestination;

        private Vector2 _grappleStartPosition;
        private Vector2 _grappleEndPosition;

        private float _grappleAngle;
        private float _initialGrappleAngle;
        private float _grappleAngleDestination;
        private sbyte _grappleDirection;

        public Player(float x, float y) : base(x, y)  
        {
            Size = new Vector2(16f, 24f);

            _stateMachine = Add(new StateMachine<State>());

            _stateMachine.SetState(State.Regular, UpdateRegular, null);
            _stateMachine.SetState(State.Aiming, UpdateAim, DrawAim, StartAiming, null);
            _stateMachine.SetState(State.Shooting, UpdateShooting, DrawGrapple, StartShooting, null);
            _stateMachine.SetState(State.Grappling, UpdateGrappling, DrawGrapple);

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
            base.Update(deltaTime);
        }

        protected override void Draw()
        {

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

                //if (velocityX == 0f)
                //    _sprite.SetAnimation("idle");
            }

            if (Grounded)
            {
                if (Input.Jump.IsPressed())
                    Jump(deltaTime);
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
            _grappleDirection = Direction;
            _grappleAngleDestination = MaxGrappleAngle;

            if (Direction >= 0)
            {
                _initialGrappleAngle = 0f;
            }
            else
            {
                float pi = Maths.PI;

                _initialGrappleAngle = pi;
                _grappleAngleDestination += pi;
            }

            _grappleAngle = _initialGrappleAngle;            
        }

        private void UpdateAim(float deltaTime)
        {
            UpdateMovement(deltaTime);

            if (Input.Grapple.IsDown())
            {
                float deltaAngle = GrappleAngleSpeed * deltaTime * _grappleDirection;

                _grappleAngle = Maths.ApproachAbs(_grappleAngle, deltaAngle, _grappleAngleDestination);
            }
            else
            {
                _anchor = null;

                if (Math.Abs(_grappleAngle - _initialGrappleAngle) < GrappleAngleDeadzone)
                    _grappleAngle = _initialGrappleAngle;

                _grappleStartPosition = Rotate(GrappleStartDistance, _grappleAngle);
                Vector2 farthestPoint = Rotate(GrappleLength, _grappleAngle);

                var grappleLine = new Line(_grappleStartPosition, farthestPoint);

                _grappleDestination = farthestPoint;

                SolidObject solid = Scene.CheckLineAllComponent<SolidObject>(grappleLine)
                    .OrderBy(anchor => Vector2.Distance(_grappleStartPosition, anchor.Position))
                    .FirstOrDefault();

                if (solid is not null)
                {
                    IEnumerable<Line> surfaces = solid.EntityRectangle.GetSurfaces()
                        .OrderBy(surface => Vector2.Distance(_grappleStartPosition, surface.GetMidPoint()));

                    foreach (Line line in surfaces)
                    {
                        if (Maths.TryGetIntersectionPoint(grappleLine, line, out Vector2 intersectionPoint))
                        {
                            _grappleDestination = intersectionPoint;

                            break;
                        }
                    }
                    
                    if (solid is Anchor anchor)
                        _anchor = anchor;
                }

                _stateMachine.ForceState(State.Shooting);
            }
        }

        private void StartShooting()
        {
            _grappleEndPosition = _grappleStartPosition;

            UpdateGrappleSprites();

            UpdatePhysics = false;
            Velocity = Vector2.Zero;
        }

        private void UpdateShooting(float deltaTime)
        {
            _grappleEndPosition = Vector2.Lerp(_grappleEndPosition, _grappleDestination, GrappleShootingLerpSpeed * deltaTime);

            float deltaX = _grappleDestination.X - _grappleEndPosition.X;
            float deltaY = _grappleDestination.Y - _grappleEndPosition.Y;

            if (Math.Abs(deltaX) < 1f && Math.Abs(deltaY) < 1f)
            {
                if (_anchor is not null)
                {
                    _stateMachine.ForceState(State.Grappling);
                }
                else
                {
                    // Wait for some time?

                    ExitGrappling();
                }
            }

            UpdateGrappleSprites();
        }

        private void UpdateGrappling(float deltaTime)
        {
            float distance = Vector2.Distance(_grappleStartPosition, _grappleDestination);

            Vector2 newGrappleStartPosition = Vector2.Lerp(_grappleStartPosition, _grappleDestination, GrapplingLerpSpeed * deltaTime / distance);
            Vector2 displacement = newGrappleStartPosition - _grappleStartPosition;

            _grappleStartPosition = newGrappleStartPosition;

            UpdateGrappleSprites();

            if (Input.Jump.IsPressed())
            {
                ExitGrappling();

                Jump(deltaTime);
            }
            else if (Input.CancelGrappling.IsPressed())
            {
                ExitGrappling();
            }
            else
            {
                Translate(displacement, out SolidObject collidedWithX, out SolidObject collidedWithY);

                if (collidedWithX == _anchor || collidedWithY == _anchor)
                {
                    ExitGrappling();

                    return;
                }
            }            
        }
        
        private void DrawAim()
        {
            float depth = Depths.WorldUI;
            Color red = Color.Red;

            float deltaAngle = Maths.DeltaAngle(_initialGrappleAngle, _grappleAngleDestination) / (AimDispayLinesCount - 1);
            deltaAngle *= -_grappleDirection;

            for (int i = 0; i < AimDispayLinesCount; i++)
            {
                float angle = _initialGrappleAngle + deltaAngle * i;

                Vector2 linePosition = Rotate(AimDisplayDistance, angle);
                Color color = Color.Lerp(Color.Yellow, red, i / (float)AimDispayLinesCount);

                Graphics.DrawLine(linePosition, angle, AimDisplayLineLength, color * AimDisplayAlpha, depth);
            }

            Vector2 aimDisplayPosition = Rotate(AimCrosshairDistance, _grappleAngle);
            Graphics.FillRectangle(Rectangle.FromCenter(aimDisplayPosition, new Vector2(2f)), red * AimDisplayAlpha, depth);
        }

        private void DrawGrapple()
        {
            _hookRopeSprite.Draw(_grappleStartPosition);
            _hookGrapleSprite.Draw(_grappleEndPosition);
        }

        private void Jump(float deltaTime)
        {
            VelocityY -= InitialThrust * deltaTime;

            _coroutineRunner.StartCoroutine(CreateJumpCoroutine());
        }

        private void UpdateGrappleSprites()
        {
            float distance = Vector2.Distance(_grappleStartPosition, _grappleEndPosition);
            var scale = new Vector2(distance / _hookRopeSprite.Texture.Width, 1f);

            _hookRopeSprite.Scale = scale;
            _hookRopeSprite.Angle = _grappleAngle;

            _hookGrapleSprite.Angle = _grappleAngle;

            _hookRopeMaterial.Scale = scale;
        }

        private void ExitGrappling()
        {
            UpdatePhysics = true;
            _stateMachine.ForceState(State.Regular);
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
            Shooting,
            Grappling
        }
    }
}
