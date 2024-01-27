using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public abstract class FakeBlock : AutoTile
    {
        public FakeBlock(string tileSet, float x, float y) : base(tileSet, x, y)
        {

        }

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Group is not null &&  Scene.CheckRectangle<Player>(Rectangle) is not null)
                Group.FadeAway();
        }
    }
}
