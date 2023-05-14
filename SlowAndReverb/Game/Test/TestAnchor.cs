using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SlowAndReverb
{
    public class TestAnchor : Entity
    {
        public TestAnchor(float x, float y) : base(x, y)
        {
            Size = new Vector2(60f, 200f);

            Add(new Anchor());
            Add(new SolidObject());
            Add(new LightOccluder());
        }

        protected override void Update(float deltaTime)
        {
            //Get<SolidObject>().Translate(new Vector2(0.5f * deltaTime, 0f));
        }

        protected override void Draw()
        {
            Graphics.FillRectangle(Rectangle, Color.CoolOrange, Depths.Blocks);
        }
    }
}
