using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public abstract class PhysicsObject : Entity
    {
        private float _accumulatorX;
        private float _accumulatorY;

        public PhysicsObject(float x, float y) : base(x, y)
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

            TranslateX(VelocityX * deltaTime);
            TranslateY(VelocityY * deltaTime);

            Ground = Scene.CheckRectangle<SolidObject>(new Rectangle(BottomLeft, BottomLeft + new Vector2(Width, 1f)));
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

        public virtual bool IsAttachedTo(SolidObject solid)
        {
            if (solid is null)
                return false;

            return solid == Ground;
        }

        // Add something like "CollisionData" as a second argument
        protected virtual void OnCollide(SolidObject with)
        {

        }

        private bool Translate(Vector2 by)
        {
            Vector2 futurePosition = Position + by;
            var futureRectangle = new Rectangle(futurePosition - HalfSize, futurePosition + HalfSize);

            IEnumerable<SolidObject> collidesWith = Scene.CheckRectangleAll<SolidObject>(futureRectangle);

            if (collidesWith.Any(solid => !solid.IgnoreCollisions))
            {
                foreach (SolidObject entity in collidesWith)
                    OnCollide(entity);

                return false;
            }

            Position = futurePosition;

            return true;
        }
    }
}
