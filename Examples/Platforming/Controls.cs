using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet.Platforming
{
    public static class Controls
    {
        public static InputProfile Profile { get; set; }

        public static VirtualButton Jump => Profile.Jump;
        public static VirtualButton Grapple => Profile.Grapple;
        public static VirtualButton CancelGrappling => Profile.CancelGrappling;
        public static VirtualButton Up => Profile.Up;
        public static VirtualButton Down => Profile.Down;
        public static VirtualAxis XAxis => Profile.XAxis;

        public static VirtualAxis MenuXAxis => Profile.MenuXAxis;
        public static VirtualAxis MenuYAxis => Profile.MenuYAxis;
    }
}
