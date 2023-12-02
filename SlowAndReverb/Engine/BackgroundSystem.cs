namespace SlowAndReverb
{
    public class BackgroundSystem : System
    {
        private readonly LayerPass _pass;

        public BackgroundSystem(Scene scene, LayerPass pass) : base(scene)
        {
            _pass = pass;
        }

        public Background CurrentBackground { get; set; }

        public override void Initialize()
        {
            _pass.Render += OnBackgroundRender;
        }

        public override void Update(float deltaTime)
        {
            //Layer foregroundLayer = Layers.Foreground;

            //Vector2 position = foregroundLayer.Camera.Position;
            //BackgroundLayer.Camera.Position = position;

            if (CurrentBackground is not null)
                CurrentBackground.Update(deltaTime);
        }

        public override void Terminate()
        {
            _pass.Render -= OnBackgroundRender;
        }

        private void OnBackgroundRender()
        {
            if (CurrentBackground is not null)
                CurrentBackground.Draw();
        }
    }
}
