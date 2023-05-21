namespace SlowAndReverb
{
    public class PauseMenu : StackMenu
    {
        public PauseMenu()
        {
            Size = new Vector2(100f, 80f);
            Position = Layers.UI.Size / 2f - HalfSize;
            Text = "Paused";

            var a = new UIMenuItem[]
            {
                new UIBars()
                {
                    Text = "A"
                },

                new UIListLeftRight<string>(new string[]
                {
                    "item1",
                    "item2",
                    "item3"
                })
                {
                    Text = "B"
                },

                new UINumericLeftRight()
                {
                    Text = "C"
                },

                new UIOnOff()
                {
                    Text = "D"
                }
            };

            for (int i = 0; i < a.Length; i++)
            {
                AddItem(a[i]);
            }
        }

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}
