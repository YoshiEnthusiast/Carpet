using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class UILeftRight : UIMenuItem
    {
        private readonly Sprite _beak;

        public UILeftRight()
        {
            _beak = new Sprite("uiBeak");
            _beak.Origin = new Vector2(_beak.Width, _beak.Height / 2f);
        }

        public string DisplayedItem { get; protected set; } = string.Empty;
        public float BeakMargin { get; set; } = 1f;

        protected abstract void OnLeftPressed();
        protected abstract void OnRightPressed();

        protected override void UpdateSelected(float deltaTime)
        {
            if (Input.IsPressed(Key.A))
                OnLeftPressed();
            else if (Input.IsPressed(Key.D))
                OnRightPressed();
        }

        protected override void DrawRightSide(float right)
        {
            float x = right;
            float centerY = ScreenY + HalfHeight;

            _beak.HorizontalEffect = SpriteEffect.None;
            _beak.Draw(x, centerY);

            x -= _beak.Width + BeakMargin;

            float textWidth = Font.MeasureWidth(DisplayedItem);
            x -= textWidth;

            Font.Draw(DisplayedItem, new Vector2(x, ScreenY), Depth);

            x -= BeakMargin;

            _beak.HorizontalEffect = SpriteEffect.Flip;
            _beak.Draw(x, centerY);
        }
    }
}
