using System.Xml;

namespace Carpet.Platforming
{
    public sealed class Game : Engine
    {
        private const int MaxLights = 100;

        private readonly StateMachine<GlobalState> _stateMachine = new();

        private Thread _contentLoadingThread;

        public Game(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(GlobalState.Loading, UpdateContentLoading, null);
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame, StartGame, null);
        }

        public static Layer ForegroundLayer { get; private set; }
        public static Layer BackgroundLayer { get; private set; }
        public static Layer ConsoleLayer { get; private set; }

        public static Pipeline Pipeline { get; private set; }

        public static LayerPass ForegroundPass { get; private set; }
        public static LayerPass ConsolePass { get; private set; }

        public static TexturePass OcclusionPass { get; private set; }
        public static TexturePass DistancePass { get; private set; }
        public static TexturePass LightPass { get; private set; }

        public static Scene CurrentScene { get; set; }
        public static bool Paused { get; set; }

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

            BackgroundLayer = new Layer(1280, 720, 0f);

            var foregroundMaterial = new ForegroundMaterial();

            ForegroundLayer = new Layer(320, 180, 1f)
            {
                Material = foregroundMaterial,
                ClearColor = new Color(40, 40, 40)
            };

            var testLayer = new Layer(520, 380, 10f)
            {
                ClearColor = Color.Black
            };

            ForegroundLayer.Camera.SetCenterOrigin();

            ConsoleLayer = new Layer(320, 180, 3f);

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
            testLayer.Camera.SetCenterOrigin();

            ConsolePass = Pipeline.AddPass(new LayerPass().SetLayer(ConsoleLayer));
            ConsolePass.Render += OnConsoleRender;

            DebugConsole.Initialize(ConsoleLayer, "testFont");

            Argument.AddType<EntityArgument>();
            DebugConsole.AddCommandContainer(typeof(TestCommands));
        }

        protected override void LoadContent()
        {
            _contentLoadingThread = new Thread(() =>
            {
                base.LoadContent();
            });

            _stateMachine.ForceState(GlobalState.Loading);

            _contentLoadingThread.Start();
        }

        private void UpdateGame(float deltaTime)
        {
            Controls.Profile.Update(deltaTime);
            DebugConsole.Update(deltaTime);

            if (Input.IsPressed(Key.Escape))
            {
                Paused = !Paused;
            }

            if (!Paused)
                CurrentScene.Update(deltaTime);
        }

        private void DrawGame()
        {
            Pipeline.Process();
        }

        private void StartGame()
        {
            CurrentScene = new DemoScene(100f);

            CurrentScene.AddSystem(new CameraSystem(0.06f, CurrentScene))
                .AddSystem(new GPULightRenderer(CurrentScene, OcclusionPass,
                    DistancePass, LightPass, MaxLights, false, ForegroundPass))
                .AddSystem(new BlockGroupsSystem(CurrentScene))
                .AddSystem(new ParticleSystem(CurrentScene, 1000));

            CurrentScene.Add(new TestEntity(0f, 0f));
            CurrentScene.Add(new TestEntity2(0f, 0f));

            for (int i = 0; i < 80; i++)
                CurrentScene.Add(new DefaultBlock(68f + i * 8f, 100));

            CurrentScene.Add(new Player(300f, -20f));
            CurrentScene.Add(new TestAnchor(200f, 20f));

            CurrentScene.Add(new FlyingLantern(80f, 80f));

            CurrentScene.Add(new Coin(150f, 60f));

            CurrentScene.Add(new TestPlatform(300f, 60f));
            CurrentScene.Add(new SpinningBoxes(600f, 30f));

            CurrentScene.Initialize();

            ForegroundLayer.Camera.Position = new Vector2(400f, 50f);
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            XmlDocument document = Utilities.LoadXML(Content.Folder + "/defaultInputSettings.xml");
            var settings = new InputSettings(document.DocumentElement);
            var profile = new InputProfile(settings);

            Controls.Profile = profile;

            _stateMachine.ForceState(GlobalState.Game);
        }

        private void OnConsoleRender()
        {
            DebugConsole.Draw();
        }
    }
}
