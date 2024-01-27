using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public abstract class Block : AutoTile
    {
        public Block(string tileSet, float x, float y) : base(tileSet, x, y)
        {
            Add(new SolidObject());
        }
    }
}
