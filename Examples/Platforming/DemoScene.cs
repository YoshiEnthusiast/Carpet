using Carpet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet.Platforming
{
    public class DemoScene : Scene
    {
        public override void Initialize(float bucketSize)
        {
            base.Initialize(bucketSize);

            float left = float.PositiveInfinity;
            float top = float.PositiveInfinity;

            float right = float.NegativeInfinity;
            float bottom = float.NegativeInfinity;

            foreach (BlockGroup group in GetEntitiesOfType<BlockGroup>())
            {
                left = Maths.Min(left, group.Left);
                top = Maths.Min(top, group.Top);

                right = Maths.Max(right, group.Right);
                bottom = Maths.Max(bottom, group.Bottom);
            }

            Layer foreground = Game.ForegroundLayer;

            float foregroundWidth = foreground.Width;
            float foregroundHeight = foreground.Height;

            float xDifference = right - left;
            float yDifference = bottom - top;   

            if (xDifference < foregroundWidth)
            {
                float value = (foregroundWidth - xDifference) / 2f;

                left -= value;
                right += value;
            }

            if (yDifference < foregroundHeight)
            {
                float value = (foregroundHeight - yDifference) / 2f;

                top -= value;
                bottom += value;
            }

            Rectangle = new Rectangle(new Vector2(left, top), new Vector2(right, bottom));

            Game.ForegroundPass.Render += Draw;
        }

        public override void Terminate()
        {
            Game.ForegroundPass.Render -= Draw;
        }
    }
}
