namespace Carpet.Examples.RayCasting
{
    public sealed class Game : Engine
    {
        private const int MaxLights = 100;
        private const int LoadingState = 0;
        private const int MainState = 1;
        private const float CameraDelta = 1.25f;

        private const int Width = 320;
        private const int Height = 180;

        private readonly StateMachine<int> _stateMachine = new();

        private Thread _contentLoadingThread;

        private Font _font;

        public Game(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(LoadingState, UpdateContentLoading, null);
            _stateMachine.SetState(MainState, UpdateGame, DrawGame, StartGame, null);
        }

        public static Layer ForegroundLayer { get; private set; }
        public static Layer UILayer { get; private set; }

        public static Pipeline Pipeline { get; private set; }

        public static LayerPass ForegroundPass { get; private set; }
        public static LayerPass UIPass { get; private set; }

        public static TexturePass OcclusionPass { get; private set; }
        public static TexturePass DistancePass { get; private set; }
        public static TexturePass LightPass { get; private set; }

        public static Scene CurrentScene { get; set; }

        protected override void Update(float deltaTime)
        {
            Input.Update();

            _stateMachine.DoUpdate(deltaTime);

            Input.Clear();
        }

        protected override void Draw()
        {
            _stateMachine.DoDraw();
        }

        protected override void OnInitialize()
        {
            Content.Initialize("Content");
            Content.LoadGraphics(TextureLoadMode.SaveAtlas);
            Graphics.Initialize();
            ShaderProgramWrapper.InitializeUniforms(GetType().Assembly);

            var foregroundMaterial = new ForegroundMaterial();

            ForegroundLayer = new Layer(320, 180, 1f)
            {
                Material = foregroundMaterial,
                ClearColor = new Color(40, 40, 40)
            };

            ForegroundLayer.Camera.SetCenterOrigin();

            UILayer = new Layer(320, 180, 3f);

            Pipeline = new Pipeline();

            OcclusionPass = GPULightRenderer.CreateOcclusionPass(320, 180, MaxLights);
            DistancePass = GPULightRenderer.CreateDistancePass(256, 100);
            LightPass = GPULightRenderer.CreateLightPass(320, 180);

            Texture2D lightMap = LightPass.GetTexture();
            foregroundMaterial.Textures[0] = lightMap;

            Pipeline.AddPass(OcclusionPass);
            Pipeline.AddPass(DistancePass);
            Pipeline.AddPass(LightPass);

            ForegroundPass = Pipeline.AddPass(new LayerPass().SetLayer(ForegroundLayer));

            UIPass = Pipeline.AddPass(new LayerPass().SetLayer(UILayer));
            UIPass.Render += RenderUI;

            _font = new Font("testFont")
            {
                OutlineWidth = 1,
                OutlineColor = Color.Black
            };
        }

        protected override void LoadContent()
        {
            _contentLoadingThread = new Thread(() =>
            {
                base.LoadContent();
            });

            _stateMachine.ForceState(LoadingState);

            _contentLoadingThread.Start();
        }

        private void UpdateGame(float deltaTime)
        {
            float delta = CameraDelta * deltaTime;
            Camera camera = ForegroundLayer.Camera;

            if (Input.IsDown(Key.A))
                camera.Position = camera.Position + new Vector2(-delta, 0f);
            else if (Input.IsDown(Key.D))
                camera.Position = camera.Position + new Vector2(delta, 0f);

            if (Input.IsDown(Key.W))
                camera.Position = camera.Position + new Vector2(0f, -delta);
            else if (Input.IsDown(Key.S))
                camera.Position = camera.Position + new Vector2(0f, delta);

            CurrentScene.Update(deltaTime);
        }

        private void DrawGame()
        {
            Pipeline.Process();
        }

        private void StartGame()
        {
            CurrentScene = new Scene(100f);

            CurrentScene.AddSystem(new GPULightRenderer(CurrentScene, OcclusionPass,
                    DistancePass, LightPass, MaxLights, false, ForegroundPass))
                .AddSystem(new ParticleSystem(CurrentScene, 1000));

            CurrentScene.Add(new Disco(-120f, 0f));

            CurrentScene.Add(new SpinningBoxes(0f, 120f));

            CurrentScene.Add(new FlyingLantern(110f, 0f));
            CurrentScene.Add(new RectangleOccluder(90f, -10f));
            CurrentScene.Add(new RectangleOccluder(125f, -5f));
            CurrentScene.Add(new RectangleOccluder(100f, 20f));

            CurrentScene.Add(new WhiteLight(220f, 35f + 120f));
            CurrentScene.Add(new CircleOccluder(220f, -8f + 120f));

            ForegroundPass.Render += CurrentScene.Draw;

            ForegroundLayer.Camera.Position = new Vector2(60f, 60f);

            CurrentScene.Initialize();
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            _stateMachine.ForceState(MainState);
        }

        private void RenderUI()
        {
            _font.Draw("WASD to move", new Vector2(10f), Color.White, 0f);
        }
    }
}
