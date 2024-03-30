using System;

namespace Carpet
{
    public class TestPlatform : Entity
    {
        public TestPlatform(float x, float y) : base(x, y)
        {
            Size = new Vector2(96f, 1f);

            var solid = new SolidObject()
            {
                CollisionLeft = false,
                CollisionRight = false,
                CollisionBottom = false
            };

            Add(solid);

            Add(new Sprite("testPlatform")
            {
                Origin = new Vector2(Width / 2f, 1f)
            });

            Add(new LightOccluder(OcclusionMode.EntityRectangle));
        }
    }
}
