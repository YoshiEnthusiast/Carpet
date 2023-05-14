using System;

namespace SlowAndReverb
{
    public class UIBars : UIMenuItem
    {
        public int Value { get; set; }
        public int BarsCount { get; set; } = 5;

        public float BarWidth { get; set; } = 4f;
        public float BarHeight { get; set; } = 8f;
        public float SpaceBetweenBars { get; set; } = 1f;

        public Color ActiveBarColor { get; set; } = Color.White;
        public Color InactiveBarColor { get; set; } = Color.Grey;

        protected override void UpdateSelected(float deltaTime)
        {
            if (Input.IsPressed(Key.A))
            {
                if (Value > 0)
                    Value--;
            }
            else if (Input.IsPressed(Key.D))
            {
                if (Value < BarsCount)
                    Value++;
            }
        }

        protected override void DrawRightSide(float right)
        {
            float x = right - BarWidth * BarsCount - SpaceBetweenBars * (BarsCount - 1);

            for (int i = 1; i <= BarsCount; i++)
            {
                if (i <= Value)
                    DrawBar(x, ActiveBarColor);
                else 
                    DrawBar(x, InactiveBarColor);

                x += BarWidth + SpaceBetweenBars;
            }
        }

        private void DrawBar(float x, Color color)
        {
            float y = ScreenY + HalfHeight - BarHeight / 2f;

            FillRectangle(new Rectangle(x, y, BarWidth, BarHeight), color);
        }
    }
}
