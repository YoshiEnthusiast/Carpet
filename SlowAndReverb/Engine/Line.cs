using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public struct Line
    {
        public Line(Vector2 start, Vector2 end)
        {
            Start = start; 
            End = end;
        }

        public Vector2 Start { get; private init; }
        public Vector2 End { get; private init; }

        public float GetLength()
        {
            return Vector2.Distance(Start, End);
        }

        public Vector2 GetMidPoint()
        {
            return (Start + End) / 2f;
        }
    }
}
