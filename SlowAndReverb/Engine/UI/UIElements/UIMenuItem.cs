namespace SlowAndReverb
{
    public class UIMenuItem : UIElement
    {
        private readonly Sprite _selectedArrow;

        public UIMenuItem()
        {
            _selectedArrow = new Sprite("uiArrow");
            _selectedArrow.Origin = new Vector2(_selectedArrow.Width, _selectedArrow.Height / 2f);

            Size = new Vector2(80f, 6f);
        }

        public bool Selected { get; set; }
        public float Indent { get; set; } = 5f;
        public float SelectedArrowMargin { get; set; } = 2f;

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Selected)
                UpdateSelected(deltaTime);
        }

        protected override void Draw()
        {
            base.Draw();

            float x = ScreenX + Indent;
            float right = ScreenRectangle.Right - Indent;

            Font.Draw(Text, new Vector2(x, ScreenY), Depth);
            DrawRightSide(right);

            if (Selected)
            {
                float centerY = ScreenY + HalfHeight;

                _selectedArrow.HorizontalEffect = SpriteEffect.None;
                _selectedArrow.Draw(ScreenX - SelectedArrowMargin, centerY);

                _selectedArrow.HorizontalEffect = SpriteEffect.Reflect;
                _selectedArrow.Draw(ScreenX + Width + SelectedArrowMargin, centerY);
            }
        }

        protected virtual void DrawRightSide(float right)
        {

        }

        protected virtual void UpdateSelected(float deltaTime)
        {

        }
    }
}
