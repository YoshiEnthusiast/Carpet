using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Carpet.Platforming
{
    public class TestAnchor : Entity
    {
        public TestAnchor(float x, float y) : base(x, y)
        {
            Size = new Vector2(60f, 200f);

            Add(new Anchor());
            Add(new SolidObject());
            Add(new LightOccluder(OcclusionMode.EntityRectangle));
        }

        protected override void Draw()
        {
            Graphics.FillRectangle(Rectangle, Color.Orange, Depths.Blocks + 10f);
        }
    }
}
