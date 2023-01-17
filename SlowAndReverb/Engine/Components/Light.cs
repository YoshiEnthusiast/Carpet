using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Light : Component
    {
        public Light(Color color, float radius)
        {
            Color = color;
            Radius = radius;
        }

        public Light()
        {

        }

        public Color Color { get; set; }

        public float Radius { get; set; }

        public Rectangle Bounds
        {
            get
            {
                Vector2 roundedPosition = Position.Round();
                Vector2 radiusVector = new Vector2(Radius);

                return new Rectangle(roundedPosition - radiusVector, roundedPosition + radiusVector);
            }
        }
    }
}
