using System;

namespace SlowAndReverb
{
    public abstract class InGameMenu : UIMenu
    {
        public InGameMenu()
        {
            BackgroundColor = Color.Lerp(Color.Grey, Color.Transparent, 0.5f);
            BorderColor = Color.White;
        }

        public float HeaderHeight { get; set; } = 9f;
        public bool DrawHeader { get; set; } = true;

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime); 
        }

        protected override void Draw()
        {
            base.Draw();

            if (DrawHeader)
            {
                float headerBottom = ScreenY + HeaderHeight;
                Graphics.DrawLine(new Vector2(ScreenX, headerBottom), new Vector2(ScreenX + Width, headerBottom), BorderColor, Depth);

                Vector2 textSize = Font.Measure(Text);
                float textX = ScreenX + (Width - textSize.X) / 2f;
                Font.Draw(Text, new Vector2(textX, headerBottom - textSize.Y + 1f), Depth);
            }
        }
    }
}
