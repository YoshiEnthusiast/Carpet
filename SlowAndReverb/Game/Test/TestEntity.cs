using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TestEntity : Entity
    {
        private readonly Font _font = new Font("testFont");

        public TestEntity(float x, float y) : base(x, y)
        {
            var sprite = new Sprite("needler", 19, 16)
            {
                Depth = 1f,
            };

            sprite.AddAnimation("amogus", new Animation(1f, true, new int[]
            {
                4,
                3,
                2,
                1,
                0
            }));

            sprite.SetAnimation("amogus");

            //Add(sprite);

            //Add(new Light()
            //{
            //    Color = Color.White,
            //    Radius = 60f
            //});
        }

        public override void Update(float deltaTime)
        {
            Position = Layers.Foreground.MousePosition;
        }

        public override void Draw()
        {
            //_font.Draw("123", Position, 1f);
        }
    }
}
