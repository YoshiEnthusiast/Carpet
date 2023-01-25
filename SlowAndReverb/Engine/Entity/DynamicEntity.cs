using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class DynamicEntity : Entity
    {
        private float _accumulatorX;
        private float _accumulatorY;

        private bool _grounded;

        public DynamicEntity(float x, float y) : base(x, y)
        {
                
        }

        public bool Grounded => _grounded;

        public Vector2 Velocity { get; set; }
        public float Weight { get; set; } = 1f;
        public float Friction { get; set; }
        public float Bounciness { get; set; }
        public bool UpdatePhysics { get; set; } = true;

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

        public override void Update(float deltaTime)
        {
            if (!UpdatePhysics)
                return;

            _grounded = Scene.CheckPointCollision<StaticEntity>(new Vector2(Position.X, Bottom + 1f)) is not null;

            if (!_grounded)
                VelocityY += Weight;

            TranslateX(VelocityX * deltaTime);
            TranslateY(VelocityY * deltaTime);
        }

        private void TranslateX(float delta)
        {
            delta += _accumulatorX;

            int distance = (int)Math.Round(delta);
            int sign = Math.Sign(distance);

            _accumulatorX = distance - delta;

            for (int i = 0; i < Math.Abs(distance); i++)
            {
                Vector2 futurePosition = Position + new Vector2(sign, 0f);
                var futureRectangle = new Rectangle(futurePosition - HalfSize, futurePosition + HalfSize);

                IEnumerable<StaticEntity> collidesWith = Scene.CheckRectangleCollisionAll<StaticEntity>(futureRectangle);
                
                if (collidesWith.Any())
                {
                    foreach (StaticEntity entity in collidesWith)
                        Collide(entity);

                    break;
                }

                Position = futurePosition;
            }
        }

        private void TranslateY(float delta)
        {
            delta += _accumulatorY;

            int distance = (int)Math.Round(delta);
            int sign = Math.Sign(distance);

            _accumulatorY = distance - delta;

            for (int i = 0; i < Math.Abs(distance); i++)
            {
                Vector2 futurePosition = Position + new Vector2(0f, sign);
                var futureRectangle = new Rectangle(futurePosition - HalfSize, futurePosition + HalfSize);

                IEnumerable<StaticEntity> collidesWith = Scene.CheckRectangleCollisionAll<StaticEntity>(futureRectangle);

                if (collidesWith.Any())
                {
                    foreach (StaticEntity entity in collidesWith)
                        Collide(entity);

                    break;
                }

                Position = futurePosition;
            }
        }

        protected virtual void Collide(StaticEntity with)
        {

        }
    }
}
