namespace SlowAndReverb
{
    public class BackgroundSystem : System
    {
        public BackgroundSystem(Scene scene) : base(scene)
        {

        }

        public Background CurrentBackground { get; set; }

        private Layer BackgroundLayer => Layers.Background;

        public override void Update(float deltaTime)
        {
            //Layer foregroundLayer = Layers.Foreground;

            //Vector2 position = foregroundLayer.Camera.Position;
            //BackgroundLayer.Camera.Position = position;

            if (CurrentBackground is not null)
                CurrentBackground.Update(deltaTime);

            if (Input.IsPressed(Key.U))
                BackgroundLayer.RenderTarget.Texture.SaveAsPng("bacl.png");
        }

        public override void OnBeforeDraw()
        {
            Graphics.BeginLayer(BackgroundLayer);

            if (CurrentBackground is not null)
                CurrentBackground.Draw();

            Graphics.EndCurrentLayer();
        }
    }
}
