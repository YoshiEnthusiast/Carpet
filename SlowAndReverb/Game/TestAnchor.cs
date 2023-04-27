using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TestAnchor : Entity
    {
        public TestAnchor(float x, float y) : base(x, y)
        {
            Size = new Vector2(60f, 100f);

            Add(new Anchor());
            Add(new LightOccluder());
        }

        protected override void Draw()
        {
            Graphics.FillRectangle(Rectangle, Color.CoolOrange, Depths.Blocks);
        }
    }
}
