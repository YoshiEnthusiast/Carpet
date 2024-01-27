using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public enum DpadState
    {
        Centered = 0b_0000_0000,

        Up = 0b_0000_0001,

        Right = 0b_0000_0010,

        Down = 0b_0000_0100,
            
        Left = 0b_0000_1000,

        RightUp = Right | Up,

        RightDown = Right | Down,

        LeftUp = Left | Up,

        LeftDown = Left | Down
    }
}
