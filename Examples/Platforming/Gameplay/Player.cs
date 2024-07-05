using System.Collections;

namespace Carpet.Platforming
{
    public class Player : PhysicsBody
    {
        #region Constants

        private const float Acceleration = 0.3f;
        private const float AirAcceleration = 0.2f;
        private const float MaxHorizontalVelocity = 3f;
        private const float Friction = 0.2f;
        private const float AirFriction = 0.1f;
        private const float GravityAcceleration = 0.2f;
        private const float WallClingingVelocityY = 0.8f;
        private const float InitialJumpThrust = -1f;
        private const float JumpThrust = -0.6f;
        private const float JumpTime = 5f;
        private const float JumpWindow = 0.07f; // Multiply Engine.TimeElapsed by _deltaTimeMultiplier...
        private const float WallJumpTime = 5f;

        private const float GrappleLength = 140f;
        private const float MaxGrappleAngle = Maths.HalfPI;
        private const float GrappleAngleSpeed = -0.05f;
        private const float GrappleAngleDeadzone = 0.3f;
        private const float GrappleStartDistance = 8f;
        private const float GrappleShootingLerpSpeed = 0.5f;
        private const float GrapplingLerpSpeed = 3f;

        private const float AimDisplayDistance = 30f;
        private const int AimDisplayLinesCount = 6;
        private const float AimDisplayLineLength = 10f;
        private const float AimDisplayAlpha = 0.6f;
        private const float AimCrosshairDistance = 27f;

        private const sbyte PositiveDirection = 1;
        private const sbyte NegativeDirection = -1;

        private readonly Vector2 _initialWallJumpThrust = new Vector2(0.7f, -0.9f);
        private readonly Vector2 _wallJumpThrust = new Vector2(0.4f, -0.6f);

        private readonly Vector2 _wallJumpHitbox = new Vector2(2f, 8f);

        #endregion

        private readonly StateMachine<State> _stateMachine;
        private readonly CoroutineRunner _coroutineRunner;
        private readonly Sprite _sprite;

        private readonly Sprite _hookRopeSprite;
        private readonly Sprite _hookGrappleSprite = new("hookGrapple");
        private readonly RepeatTextureMaterial _hookRopeMaterial = new();

        private double _pressedJumpAt = double.NegativeInfinity;
        private float _jumpTimer;
        private bool _jumping;

        private Anchor _anchor;
        private Vector2 _perviousAnchorPosition;
        private Vector2 _grappleDestination;

        private Vector2 _grappleStartPosition;
        private Vector2 _grappleEndPosition;

        private float _grappleAngle;
        private float _initialGrappleAngle;
        private float _grappleAngleDestination;
        private sbyte _grappleDirection;

        private readonly List<Entity> _entitiesBuffer = [];

        public Player(float x, float y) : base(x, y)  
        {
            Size = new Vector2(16f, 24f);
            Direction = 1;

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

            Add(new BloomPoint()
            {
                Radius = 70f,
                Color = new Color(150),
                Volume = 0f
            });
            
            Add(new Light()
            {
                Radius = 40f,
                Color = new Color(50),
                Volume = 0.1f
            });
        }

        public sbyte Direction { get; set; } = 1;
        public bool FacingLeft => Direction < 0;
        public bool FacingRight => Direction > 0;

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsDown(Key.O))
                Game.ForegroundLayer.Camera.Zoom += 0.1f;
        }

        protected override void Draw()
        {
            
        }

        private void UpdateMovement(float deltaTime)
        {
            float velocityX = VelocityX;
            float xAxis = Controls.XAxis.GetValue();

            if (xAxis != 0f)
            {
                float acceleration = GetPhysicsValue(Acceleration, AirAcceleration) * deltaTime;

                Direction = (sbyte)Controls.XAxis.GetSign();
                VelocityX = Maths.ApproachAbs(velocityX, acceleration * xAxis, MaxHorizontalVelocity);

                _sprite.SetAnimation(null);
            }
            else
            {
                float friction = GetPhysicsValue(Friction, AirFriction) * deltaTime;

                if (velocityX > 0f)
                    VelocityX = Maths.Max(velocityX - friction, 0f);
                else
                    VelocityX = Maths.Min(velocityX + friction, 0f);

                //if (velocityX == 0f)
                //    _sprite.SetAnimation("idle");
            }

            bool jumpPressed = Controls.Jump.IsPressed();

            if (jumpPressed)
                _pressedJumpAt = Engine.TimeElapsed;

            if (Grounded)
            {
                VelocityY = 0f;

                if (!_jumping && (jumpPressed || Engine.TimeElapsed - _pressedJumpAt < JumpWindow))
                {
                    _pressedJumpAt = double.NegativeInfinity;
                    Jump();
                }
            }
            else
            {
                float gravityAcceleration = GravityAcceleration * deltaTime;

                float width = _wallJumpHitbox.X;
                float height = _wallJumpHitbox.Y;

                float y = Y - height / 2f;

                var left = new Rectangle(Left - width, y, width, height);
                var right = new Rectangle(Right, y, width, height);

                SolidObject leftSolid = Scene.CheckRectangleComponent<SolidObject>(left);
                SolidObject rightSolid = Scene.CheckRectangleComponent<SolidObject>(right);

                bool leftWall = leftSolid is not null && leftSolid.CollisionRight;
                bool rightWall = rightSolid is not null && rightSolid.CollisionLeft;

                if (leftWall || rightWall)
                {
                    if (VelocityY >= 0f && (leftWall && Touches(leftSolid.Entity) || rightWall && Touches(rightSolid.Entity)))
                        VelocityY = WallClingingVelocityY * deltaTime;
                    else
                        VelocityY += gravityAcceleration;

                    if (jumpPressed)
                    {
                        sbyte jumpDirection = Direction;

                        if (leftWall && leftSolid.AllowWallJump)
                        {
                            jumpDirection = PositiveDirection;
                            WallJump(leftSolid, jumpDirection);
                        }
                        else if (rightSolid.AllowWallJump)
                        {
                            jumpDirection = NegativeDirection;
                            WallJump(rightSolid, jumpDirection);
                        }

                        Direction = jumpDirection;
                    }
                }
                else
                {
                    VelocityY += gravityAcceleration;
                }
            }
        }

        private void UpdateRegular(float deltaTime)
        {
            UpdateMovement(deltaTime);

            if (Controls.Grapple.IsPressed())
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

            if (Controls.Grapple.IsDown())
            {
                if (Controls.CancelGrappling.IsPressed())
                {
                    _stateMachine.ForceState(State.Regular);

                    return;
                }

                float deltaAngle = GrappleAngleSpeed * deltaTime * _grappleDirection;

                _grappleAngle = Maths.ApproachAbs(_grappleAngle, deltaAngle, _grappleAngleDestination);
            }
            else
            {
                _anchor = null;

                if (Maths.Abs(_grappleAngle - _initialGrappleAngle) < GrappleAngleDeadzone)
                    _grappleAngle = _initialGrappleAngle;

                _grappleStartPosition = Rotate(GrappleStartDistance, _grappleAngle);
                Vector2 farthestPoint = Rotate(GrappleLength, _grappleAngle);

                var grappleLine = new Line(_grappleStartPosition, farthestPoint);

                _grappleDestination = farthestPoint;

                IEnumerable<Entity> entities = Scene.CheckLineAll<Entity>(grappleLine, _entitiesBuffer)
                    .OrderBy(entity => Vector2.Distance(_grappleStartPosition, entity.Position));

                foreach (Entity entity in entities)
                {
                    Grapplable grapplable = entity.Get<Grapplable>();

                    if (grapplable is not null)
                        grapplable.Grappled(this);

                    Anchor anchor = entity.Get<Anchor>();

                    bool solidExists = entity.Has<SolidObject>();
                    bool anchorExists = anchor is not null;

                    if (solidExists || anchorExists)
                    {
                        IEnumerable<Line> surfaces = GetGrappleSurfaces(entity)
                            .OrderBy(surface => Vector2.Distance(_grappleStartPosition, surface.GetMidPoint()));

                        foreach (Line line in surfaces)
                        {
                            if (Maths.TryGetIntersectionPoint(grappleLine, line, out Vector2 intersectionPoint))
                            {
                                _grappleDestination = intersectionPoint;

                                break;
                            }
                        }

                        if (anchorExists)
                        {
                            _anchor = anchor;

                            _perviousAnchorPosition = anchor.Position;
                        }
                    }
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

            if (Maths.Abs(deltaX) < 1f && Maths.Abs(deltaY) < 1f)
            {
                if (_anchor is not null)
                {
                    _stateMachine.ForceState(State.Grappling);
                }
                else
                {
                    ExitGrappling();
                }
            }

            UpdateGrappleSprites();
        }

        private void UpdateGrappling(float deltaTime)
        {
            if (Controls.Jump.IsPressed())
            {
                ExitGrappling();

                Jump();
            }
            else if (Controls.CancelGrappling.IsPressed())
            {
                ExitGrappling();
            }
            else
            {
                Vector2 anchorPosition = _anchor.Position;

                _grappleDestination += anchorPosition - _perviousAnchorPosition;
                _grappleEndPosition = _grappleDestination;
                _perviousAnchorPosition = anchorPosition;

                float distance = Vector2.Distance(_grappleStartPosition, _grappleDestination);

                Vector2 previousPosition = Position;

                Vector2 newGrappleStartPosition = Vector2.Lerp(_grappleStartPosition, _grappleDestination, GrapplingLerpSpeed * deltaTime / distance);
                Vector2 moveBy = newGrappleStartPosition - _grappleStartPosition;

                Translate(moveBy);

                Vector2 displacement = Position - previousPosition;
                _grappleStartPosition += displacement;

                _grappleAngle = Maths.Atan2(_grappleStartPosition, _grappleDestination);

                if (Touches(_anchor.EntityRectangle) || displacement == Vector2.Zero)
                    ExitGrappling();

                UpdateGrappleSprites();
            }            
        }
        
        private void DrawAim()
        {
            float depth = Depths.WorldUI;
            Color red = Color.Red;

            float deltaAngle = Maths.DeltaAngle(_initialGrappleAngle, _grappleAngleDestination) / (AimDisplayLinesCount - 1);
            deltaAngle *= -_grappleDirection;

            for (int i = 0; i < AimDisplayLinesCount; i++)
            {
                float angle = _initialGrappleAngle + deltaAngle * i;

                Vector2 linePosition = Rotate(AimDisplayDistance, angle);
                Color color = Color.Lerp(Color.Yellow, red, i / (float)AimDisplayLinesCount);

                Graphics.DrawLine(linePosition, angle, AimDisplayLineLength, color * AimDisplayAlpha, depth);
            }

            Vector2 aimDisplayPosition = Rotate(AimCrosshairDistance, _grappleAngle);
            Graphics.FillRectangle(Rectangle.FromCenter(aimDisplayPosition, new Vector2(2f)), red * AimDisplayAlpha, depth);
        }

        private void DrawGrapple()
        {
            _hookRopeSprite.Draw(_grappleStartPosition);
            _hookGrappleSprite.Draw(_grappleEndPosition);
        }

        private void Jump()
        {
            PerformJump(new Vector2(0f, InitialJumpThrust), new Vector2(0f, JumpThrust), JumpTime);
        }

        private void WallJump(SolidObject wall, sbyte direction)
        {
            PerformJump(new Vector2(_initialWallJumpThrust.X * direction, _initialWallJumpThrust.Y), new Vector2(_wallJumpThrust.X * direction, _wallJumpThrust.Y), WallJumpTime);
        }

        private void PerformJump(Vector2 initialThrust, Vector2 thrust, float jumpTime)
        {
            Velocity += initialThrust;
            _jumping = true;

            _coroutineRunner.StartCoroutine(CreateJumpCoroutine(thrust, jumpTime));
        }

        private void UpdateGrappleSprites()
        {
            float distance = Vector2.Distance(_grappleStartPosition, _grappleEndPosition);
            var scale = new Vector2(distance / _hookRopeSprite.Texture.Width, 1f);

            _hookRopeSprite.Scale = scale;
            _hookRopeSprite.Angle = _grappleAngle;

            _hookGrappleSprite.Angle = _grappleAngle;

            _hookRopeMaterial.Scale = scale;
        }

        private void ExitGrappling()
        {
            UpdatePhysics = true;
            _stateMachine.ForceState(State.Regular);
        }

        private IEnumerable<Line> GetGrappleSurfaces(Entity entity) 
        {
            SolidObject solid = entity.Get<SolidObject>();

            if (solid is not null && !entity.Has<Anchor>())
            {
                Rectangle rectangle = entity.Rectangle;

                float x = _grappleStartPosition.X;
                float y = _grappleStartPosition.Y;

                if (solid.CollisionLeft && x <= rectangle.Left)
                    yield return rectangle.LeftSurface;

                if (solid.CollisionTop && y <= rectangle.Top)
                    yield return rectangle.TopSurface;

                if (solid.CollisionRight && x >= rectangle.Right)
                    yield return rectangle.RightSurface;

                if (solid.CollisionBottom && y >= rectangle.Bottom)
                    yield return rectangle.BottomSurface;

                yield break;
            }

            foreach (Line surface in entity.Rectangle.GetSurfaces())
                yield return surface;
        }

        private float GetPhysicsValue(float grounded, float air)
        {
            if (Grounded)
                return grounded;

            return air;
        }

        private IEnumerator CreateJumpCoroutine(Vector2 thrust, float jumpTime)
        {
            _jumpTimer = jumpTime;

            while (true)
            {
                if (!Controls.Jump.IsDown() || _jumpTimer <= 0f)
                {
                    _jumping = false;

                    yield break;
                }

                float deltaTime = Engine.DeltaTime;

                Velocity += thrust * deltaTime;
                _jumpTimer -= deltaTime;

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
