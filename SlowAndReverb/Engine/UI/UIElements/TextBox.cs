using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class TextBox : UIElement
    {
        protected override void Update(float deltaTime)
        {
            Text = Input.UpdateTextInputString(Text);
        }

        protected override void Draw() 
        {
            base.Draw();

            Vector2 textPosition = ScreenRectangle.BottomLeft + new Vector2(BorderWidth, -BorderWidth) + new Vector2(1f, -1f);
            textPosition -= new Vector2(0f, Font.LineHeight);

            Font.Draw(Text, textPosition, Depth);
        }
    }
}
