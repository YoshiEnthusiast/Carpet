using System;
using System.Threading;
using System.Xml;

namespace Carpet
{
    public sealed class Demo : Engine
    {
        private readonly StateMachine<GlobalState> _stateMachine = new StateMachine<GlobalState>();

        private Thread _contentLoadingThread;

        public Demo(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(GlobalState.Loading, UpdateContentLoading, null);
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame, StartGame, null);
        }

        public static SmoothCameraLayer ForegroundLayer { get; private set; }
        public static Layer BackgroundLayer { get; private set; }
        public static Layer ConsoleLayer { get; private set; }

        public static RenderTarget OccludeBuffer { get; private set; }
        public static RenderTarget ShadowBuffer { get; private set; }
        public static RenderTarget Lightmap { get; private set; }

        public static Pipeline Pipeline { get; private set; }

        public static LayerPass ForegroundPass { get; private set; }
        public static LayerPass BackgroundPass { get; private set; }
        public static LayerPass ConsolePass { get; private set; }
        public static Pass OccludeBufferPass { get; private set; }
        public static Pass ShadowBufferPass { get; private set; }
        public static Pass LightmapPass { get; private set; }

        public static Scene CurrentScene { get; set; }
        public static bool Paused { get; set; }

        protected override void Update(float deltaTime)
        {
            _stateMachine.DoUpdate(deltaTime);
        }

        protected override void Draw()
        {
            _stateMachine.DoDraw();
        }

        protected override void OnInitialize()
        {
            Lightmap = RenderTarget.FromTexture(324, 184);

            // 2240
            ShadowBuffer = RenderTarget.FromTexture(10_000, 10_000);
            OccludeBuffer = RenderTarget.FromTexture(2240, 2240);

            BackgroundLayer = new Layer(1280, 720, 0f);

            ForegroundLayer = new SmoothCameraLayer(324, 184, 320, 180, 1f)
            {
                Material = new ForegroundMaterial(),
                ClearColor = new Color(40, 40, 40)
            };

            ForegroundLayer.Camera.SetCenterOrigin();

            ConsoleLayer = new Layer(320, 180, 3f);

            Pipeline = new Pipeline();

            OccludeBufferPass = Pipeline.AddPass(new Pass(BlendMode.Additive)
                .SetRenderTarget(OccludeBuffer));

            ShadowBufferPass = Pipeline.AddPass(new Pass(BlendMode.Additive)
                .SetRenderTarget(ShadowBuffer));

            LightmapPass = Pipeline.AddPass(new Pass(BlendMode.Additive)
                .SetRenderTarget(Lightmap));

            BackgroundPass = Pipeline.AddPass(new LayerPass().SetLayer(BackgroundLayer));
            ForegroundPass = Pipeline.AddPass(new LayerPass().SetLayer(ForegroundLayer));

            ConsolePass = Pipeline.AddPass(new LayerPass().SetLayer(ConsoleLayer));
            ConsolePass.Render += OnConsoleRender;

            Editor.Initialize();

            DebugConsole.Initialize(ConsoleLayer, "testFont");

            Argument.AddType<Game.Entity>();
            DebugConsole.AddCommandContainer(typeof(TestCommands));
        }

        protected override void LoadContent()
        {
            _contentLoadingThread = new Thread(() =>
            {
                // Track progress

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
            //CurrentScene.Draw();
            Pipeline.Process();
        }

        private void StartGame()
        {
            CurrentScene = new Scene();

            CurrentScene.Add(new TestEntity(0f, 0f));
            CurrentScene.Add(new TestEntity2(0f, 0f));
            //Scene.Add(new TestEntity2(180f, 90f));
            //Scene.Add(new Platform(120f, 100f));
            
            // 25
            for (int i = 0; i < 80; i++)
                CurrentScene.Add(new DefaultBlock(68f + i * 8f, 100)); // 148

            //Scene.Add(new Block("tileset", 68f, 140f));
            //Scene.Add(new Block("tileset", 68f + 8f * 25f, 140f));

            CurrentScene.Add(new Player(300f, -20f));
            CurrentScene.Add(new TestAnchor(200f, 20f));

            CurrentScene.Add(new FlyingLantern(80f, 80f));

            CurrentScene.Add(new Coin(150f, 60f));
            //CurrentScene.Add(new Coin(230f, 80f));

            CurrentScene.Add(new TestPlatform(300f, 60f));

            Palette palette = Content.GetPalette("test2");
            CurrentScene.SetPalette(palette);

            //CurrentScene.Add(new Boids(0f, 0f)
            //{
            //    Size = new Vector2(600f, 300f),
            //    Position = new Vector2(400f, 20f)
            //});

            
            CurrentScene.Initialize();
            ForegroundLayer.Camera.Position = new Vector2(400f, 50f);
            //CurrentScene.GetSystem<BackgroundSystem>().CurrentBackground = new TestBackground();
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            Console.WriteLine("Content loaded");
                                                            //TODO: utility method
            XmlDocument document = Utilities.LoadXML(Content.Folder + "\\defaultInputSettings.xml");
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
