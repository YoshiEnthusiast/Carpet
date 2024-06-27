using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet.Platforming
{
    public class TestEntity2 : Entity
    {
        public TestEntity2(float x, float y) : base(x, y)
        {
            Position = new Vector2(400f, 45f);
            Size = new Vector2(32f);

            Add(new Sprite("Yosh"));
        }
    }
}
