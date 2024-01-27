using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class TestEntity2 : Entity
    {
        public TestEntity2(float x, float y) : base(x, y)
        {
            //var sprite = new Sprite("burn")
            //{
            //    Depth = 1f
            //};

            Position = new Vector2(400f, 45f);
            Size = new Vector2(32f);

            Add(new Sprite("Yosh"));
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
