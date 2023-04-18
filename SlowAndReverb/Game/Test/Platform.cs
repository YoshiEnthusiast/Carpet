using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Platform : Entity
    {
        //[[164 93] [196 125]]
        //[[60 125] [260 175]]

        public Platform(float x, float y) : base(x, y)
        {
            Size = new Vector2(20f);

            Console.WriteLine(Top);
        }

        protected override void Draw()
        {

        }
    }
}
