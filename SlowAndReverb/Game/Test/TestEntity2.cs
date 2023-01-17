using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TestEntity2 : StaticEntity
    {
        public TestEntity2(float x, float y) : base(x, y)
        {
            var sprite = new Sprite("burn")
            {
                Depth = 1f
            };

            Position = new Vector2(150f, 45f);
            Size = new Vector2(32f);
            //Weight = 10f;

            //Add(sprite);

            //Add(new Light(Color.LightGreen, 100f));
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Draw()
        {
            DrawCollision(5f);
            //Graphics.DrawCircle(Layers.Foreground.MousePosition, Color.Pink, 20, 30f);
        }
    }
}
