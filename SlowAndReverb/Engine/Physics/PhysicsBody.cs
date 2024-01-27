using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Carpet
{
    public class PhysicsBody : Entity
    {
        private readonly Accumulator _accumulator = new Accumulator();

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

            Translate(Velocity);

            Ground = FindGround();
        }

        public void Translate(Vector2 by)
        {
            TranslateX(by.X);
            TranslateY(by.Y);
        }

        public void TranslateX(float by)
        {
            _accumulator.ChargeX(by);

            int distance = _accumulator.ReleaseX();
            int sign = Maths.Sign(distance);

            for (int i = 0; i < Maths.Abs(distance); i++)
            {
                if (!TranslateByOneX(sign))
                {
                    if (UpdatePhysics)
                        VelocityX = 0f;

                    break;
                }
            }
        }

        public void TranslateY(float by)
        {
            _accumulator.ChargeY(by);

            int distance = _accumulator.ReleaseY();
            int sign = Maths.Sign(distance);

            for (int i = 0; i < Maths.Abs(distance); i++)
            {
                if (!TranslateByOneY(sign))
                {
                    if (UpdatePhysics)
                        VelocityY = 0f;

                    break;
                }
            }
        }

        public bool TranslateByOneX(int sign)
        {
            var translation = new Vector2(sign, 0f);

            Rectangle futureRectangle = Rectangle.Translate(translation);
            IEnumerable<SolidObject> touches = Scene.CheckRectangleAllComponent<SolidObject>(futureRectangle);

            foreach (SolidObject solid in touches)
                if (solid.CheckHorizontalStaticCollision(sign, this))
                    return false;

            Position += translation;

            return true;
        }

        public bool TranslateByOneY(int sign)
        {
            var translation = new Vector2(0f, sign);

            Rectangle futureRectangle = Rectangle.Translate(translation);
            IEnumerable<SolidObject> touches = Scene.CheckRectangleAllComponent<SolidObject>(futureRectangle);

            foreach (SolidObject solid in touches)
                if (solid.CheckVerticalStaticCollision(sign, this))
                    return false;

            Position += translation;

            return true;
        }

        public virtual bool IsAttachedTo(SolidObject solid)
        {
            if (solid is null)
                return false;

            return Ground == solid;
        }

        private SolidObject FindGround()
        {
            Vector2 bottomLeft = BottomLeft;
            var rectangle = new Rectangle(bottomLeft, bottomLeft + new Vector2(Width, 1f));

            IEnumerable<SolidObject> solids = Scene.CheckRectangleAllComponent<SolidObject>(rectangle);

            foreach (SolidObject solid in solids)
            {
                if (solid.CollisionTop && Bottom <= solid.EntityRectangle.Top)
                {
                    return solid;
                }
            }

            return null;
        }
    }
}
