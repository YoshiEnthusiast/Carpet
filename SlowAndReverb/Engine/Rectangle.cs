using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public struct Rectangle
    {
        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; init; }
        public float Y { get; init; }
        public float Width { get; init; }
        public float Height { get; init; }    
    }
}
