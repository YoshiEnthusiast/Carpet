using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TestEntity2 : SolidObject
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

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            //Graphics.DrawCircle(Layers.Foreground.MousePosition, Color.Pink, 20, 30f);
        }
    }
}
