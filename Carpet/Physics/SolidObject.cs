using System.Collections.Generic;
using System.Linq;

namespace Carpet
{
    public class SolidObject : Component
    {
        private const int PositiveSign = 1;
        private const int NegativeSign = -1;

        private readonly Accumulator _accumulator = new();

        public bool CollisionLeft { get; set; } = true;
        public bool CollisionTop { get; set; } = true;
        public bool CollisionRight { get; set; } = true;
        public bool CollisionBottom { get; set; } = true;

        public bool AllowWallJump { get; set; } = true;

        public void Translate(Vector2 by)
        {
            _accumulator.Charge(by);

            Vector2 translation = _accumulator.Release();

            int horizontalSign = Maths.Sign(translation.X);
            int verticalSign = Maths.Sign(translation.Y);

            Entity.Position += translation;

            foreach (PhysicsBody physicsBody in Scene.Entities.OfType<PhysicsBody>())
            {
                if (CheckApproachingCollision(horizontalSign, verticalSign, physicsBody) || physicsBody.IsAttachedTo(this))
                {
                    Vector2 destination = physicsBody.Position + translation;
                    physicsBody.Translate(translation);

                    if (physicsBody.Position != destination)
                    {
                        // TODO: Squeeze
                    }
                }
            }    
        }

        public void TranslateX(float by)
        {
            Translate(new Vector2(by, 0f));
        }

        public void TranslateY(float by)
        {
            Translate(new Vector2(0f, by));
        }

        public bool CheckHorizontalStaticCollision(int entitySign, PhysicsBody physicsBody)
        {
            Rectangle physicsBodyRectangle = physicsBody.Rectangle;

            if (EntityRectangle.Intersects(physicsBodyRectangle))
                return false;

            Rectangle rectangle = physicsBodyRectangle.TranslateX(entitySign);

            float entityLeft = rectangle.Left;
            float entityRight = rectangle.Right;

            float left = EntityRectangle.Left;
            float right = EntityRectangle.Right;

            if (entitySign == PositiveSign && CollisionLeft)
            {
                if (entityRight > left && entityLeft < left)
                    return true;
            }
            else if (entitySign == NegativeSign && CollisionRight)
            {
                if (entityLeft < right && entityRight > right)
                    return true;
            }

            return false;
        }

        public bool CheckVerticalStaticCollision(int entitySign, PhysicsBody physicsBody)
        {
            Rectangle physicsBodyRectangle = physicsBody.Rectangle;

            if (EntityRectangle.Intersects(physicsBodyRectangle))
                return false;

            Rectangle futureRectangle = physicsBodyRectangle.TranslateY(entitySign);

            float entityTop = futureRectangle.Top;
            float entityBottom = futureRectangle.Bottom;

            float top = EntityRectangle.Top;
            float bottom = EntityRectangle.Bottom;

            if (entitySign == PositiveSign && CollisionTop)
            {
                if (entityBottom > top && entityTop < top)
                    return true;
            }
            else if (entitySign == NegativeSign && CollisionBottom)
            {
                if (entityTop < bottom && entityBottom > bottom)
                    return true;
            }

            return false;
        }

        public IEnumerable<Line> GetCollisionSurfaces()
        {
            if (CollisionLeft)
                yield return EntityRectangle.LeftSurface;

            if (CollisionTop)
                yield return EntityRectangle.TopSurface;    

            if (CollisionRight)
                yield return EntityRectangle.RightSurface;

            if (CollisionBottom)
                yield return EntityRectangle.BottomSurface;
        }

        private bool CheckHorizontalApproachingCollision(int sign, PhysicsBody physicsBody)
        {
            Rectangle rectangle = physicsBody.Rectangle;

            float entityLeft = rectangle.Left;
            float entityRight = rectangle.Right;

            float left = EntityRectangle.Left;
            float right = EntityRectangle.Right;

            if (sign == PositiveSign && CollisionRight)
            {
                if (entityLeft < right && entityRight > right)
                    return true;
            }
            else if (sign == NegativeSign && CollisionLeft)
            {
                if (entityRight > left && entityLeft < left)
                    return true;
            }

            return false;
        }

        private bool CheckVerticalApproachingCollision(int sign, PhysicsBody physicsBody)
        {
            Rectangle rectangle = physicsBody.Rectangle;

            float entityTop = rectangle.Top;
            float entityBottom = rectangle.Bottom;

            float top = EntityRectangle.Top;
            float bottom = EntityRectangle.Bottom;

            if (sign == PositiveSign && CollisionBottom)
            {
                if (entityTop < bottom && entityBottom > bottom)
                    return true;
            }
            else if (sign == NegativeSign && CollisionTop)
            {
                if (entityBottom > top && entityTop < top)
                    return true;
            }

            return false;
        }

        private bool CheckApproachingCollision(int horizontalSign, int verticalSign, PhysicsBody physicsBody)
        {
            return CheckHorizontalApproachingCollision(horizontalSign, physicsBody) || CheckVerticalApproachingCollision(verticalSign, physicsBody);
        } 
    }
}
