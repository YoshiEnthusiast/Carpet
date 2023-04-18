using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class PhysicsBody : Component
    {
        private float _accumulatorX;
        private float _accumulatorY;

        public PhysicsBody()
        {
            AttachedFunction = IsAttachedTo;
        }

        public float VelocityX
        {
            get
            {
                return Velocity.X;
            }

            set
            {
                Velocity = new Vector2(value, Velocity.Y);
            }
        }

        public float VelocityY
        {
            get
            {
                return Velocity.Y;
            }

            set
            {
                Velocity = new Vector2(Velocity.X, value);
            }
        }

        public Vector2 Velocity { get; set; }
        public bool UpdatePhysics { get; set; } = true;

        public AttachedFunction AttachedFunction { get; set; }
        public SolidObject Ground { get; private set; }

        public AttachedFunction DefaultAttachedFunction => IsAttachedTo;
        public bool Grounded => Ground is not null;

        protected override void Update(float deltaTime)
        {
            if (!UpdatePhysics)
                return;

            TranslateX(VelocityX * deltaTime);
            TranslateY(VelocityY * deltaTime);

            Vector2 bottomLeft = Entity.BottomLeft;
            var rectangle = new Rectangle(bottomLeft, bottomLeft + new Vector2(Entity.Width, 1f));

            Ground = Scene.CheckRectangleComponent<SolidObject>(rectangle);
        }

        public void TranslateX(float by)
        {
            _accumulatorX += by;

            int distance = (int)Math.Round(_accumulatorX);
            int sign = Math.Sign(distance);

            _accumulatorX -= distance;

            for (int i = 0; i < Math.Abs(distance); i++)
            {
                var translation = new Vector2(sign, 0f);

                if (!Translate(translation))
                {
                    VelocityX = 0f;

                    break;
                }
            }
        }

        public void TranslateY(float by)
        {
            _accumulatorY += by;

            int distance = (int)Math.Round(_accumulatorY);
            int sign = Math.Sign(distance);

            _accumulatorY -= distance;

            for (int i = 0; i < Math.Abs(distance); i++)
            {
                var translation = new Vector2(0f, sign);

                if (!Translate(translation))
                {
                    VelocityY = 0f;

                    break;
                }
            }
        }

        private bool Translate(Vector2 by)
        {
            Vector2 futurePosition = Position + by;
            Vector2 halfSize = Entity.HalfSize;

            var futureRectangle = new Rectangle(futurePosition - halfSize, futurePosition + halfSize);

            IEnumerable<SolidObject> collidesWith = Scene.CheckRectangleAllComponent<SolidObject>(futureRectangle);

            if (collidesWith.Any())
            {
                // onCollide callback or something

                return false;
            }

            Entity.Position = futurePosition;

            return true;
        }

        private bool IsAttachedTo(PhysicsBody physicsBody, SolidObject solid)
        {
            if (solid is null)
                return false;

            return physicsBody.Ground == solid;
        }
    }
}
