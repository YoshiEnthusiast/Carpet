using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class PhysicsBody : Entity
    {
        private float _accumulatorX;
        private float _accumulatorY;

        public PhysicsBody(float x, float y) : base(x, y)
        {

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

        public SolidObject Ground { get; private set; }
        public bool Grounded => Ground is not null;

        protected override void Update(float deltaTime)
        {
            if (!UpdatePhysics)
                return;

            Translate(Velocity * deltaTime);

            Vector2 bottomLeft = BottomLeft;
            var rectangle = new Rectangle(bottomLeft, bottomLeft + new Vector2(Width, 1f));

            Ground = Scene.CheckRectangleComponent<SolidObject>(rectangle);
        }

        public void Translate(Vector2 by, out SolidObject collidedWithX, out SolidObject collidedWithY)
        {
            TranslateX(by.X, out collidedWithX);
            TranslateY(by.Y, out collidedWithY);
        }

        public void TranslateX(float by, out SolidObject collidedWith)
        {
            _accumulatorX += by;

            int distance = (int)Math.Round(_accumulatorX);
            int sign = Math.Sign(distance);

            collidedWith = default;

            _accumulatorX -= distance;

            for (int i = 0; i < Math.Abs(distance); i++)
            {
                var translation = new Vector2(sign, 0f);

                if (!TranslateInstantly(translation, out collidedWith))
                {
                    if (UpdatePhysics)
                        VelocityX = 0f;

                    break;
                }
            }
        }

        public void TranslateY(float by, out SolidObject collidedWith)
        {
            _accumulatorY += by;

            int distance = (int)Math.Round(_accumulatorY);
            int sign = Math.Sign(distance);

            _accumulatorY -= distance;

            collidedWith = default;

            for (int i = 0; i < Math.Abs(distance); i++)
            {
                var translation = new Vector2(0f, sign);

                if (!TranslateInstantly(translation, out collidedWith))
                {
                    if (UpdatePhysics)
                        VelocityY = 0f;

                    break;
                }
            }
        }

        public void Translate(Vector2 by)
        {
            Translate(by, out _, out _);
        }

        public void TranslateX(float by)
        {
            TranslateX(by, out _);
        }

        public void TranslateY(float by)
        {
            TranslateY(by, out _);
        }

        public bool TranslateInstantly(Vector2 by, out SolidObject collidedWith)
        {
            Vector2 futurePosition = Position + by;
            Vector2 halfSize = HalfSize;

            var futureRectangle = new Rectangle(futurePosition - halfSize, futurePosition + halfSize);

            IEnumerable<SolidObject> collidesWith = Scene.CheckRectangleAllComponent<SolidObject>(futureRectangle);

            if (collidesWith.Any())
            {
                collidedWith = collidesWith.First();

                return false;
            }

            Position = futurePosition;
            collidedWith = default;

            return true;
        }

        public virtual bool IsAttachedTo(SolidObject solid)
        {
            if (solid is null)
                return false;

            return Ground == solid;
        }
    }
}
