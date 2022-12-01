using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public readonly record struct Character(Rectangle TextureBounds, Vector2 Bearing, int Advance);
}
