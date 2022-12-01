using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    //TODO: Написать свою реализацию этого всего

    public static class Maths
    {
        public const float PI = 3.141593f;

        public static float Sin(float angle)
        {
            return (float)Math.Sin(angle);
        }

        public static float Cos(float angle)
        {
            return (float)Math.Cos(angle);
        }

        public static float Tan(float angle)
        {
            return (float)Math.Tan(angle);    
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static int Ceiling(float value)
        {
            return (int)Math.Ceiling(value);
        }

        public static int Floor(float value)
        {
            return (int)Math.Floor(value);
        }
    }
}
