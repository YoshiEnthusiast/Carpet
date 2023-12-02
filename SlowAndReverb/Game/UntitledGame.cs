using System;
using System.Threading;

namespace SlowAndReverb
{
    public sealed class UntitledGame : Engine
    {
        private readonly StateMachine<GlobalState> _stateMachine = new StateMachine<GlobalState>();

        private readonly UIRoot _pauseRoot = new UIRoot();
        private InGameMenu _pauseMenu;

        private Thread _contentLoadingThread;

        public UntitledGame(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(GlobalState.Loading, UpdateContentLoading, null);
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame, StartGame, null);
        }

        public static Pipeline Pipeline { get; private set; }

        public static LayerPass ForegroundPass { get; private set; }
        public static Pass OccluderBufferPass { get; private set; }
        public static Pass ShadowBufferPass { get; private set; }
        public static Pass LightMapPass { get; private set; }

        public static Scene CurrentScene { get; set; }
        public static bool Paused { get; set; }

        protected override void Update(float deltaTime)
        {
            _stateMachine.DoUpdate(deltaTime);

            UI.Update(deltaTime);
        }

        protected override void Draw()
        {
            _stateMachine.DoDraw();

            UI.Draw();
        }

        protected override void OnInitialize()
        {
            RenderTargets.Initialize();
            Layers.Initialize();

            Pipeline = new Pipeline();

            OccluderBufferPass = Pipeline.AddPass(new Pass(BlendMode.Additive, Color.Transparent)
                .SetRenderTarget(RenderTargets.OccluderBuffer));

            ShadowBufferPass = Pipeline.AddPass(new Pass(BlendMode.Additive, Color.Transparent)
                .SetRenderTarget(RenderTargets.ShadowBuffer));

            LightMapPass = Pipeline.AddPass(new Pass(BlendMode.Additive, Color.Transparent)
                .SetRenderTarget(RenderTargets.LightMap));

            ForegroundPass = Pipeline.AddPass(new LayerPass().SetLayer(Layers.Foreground));

            UI.Initialize();
            Editor.Initialize();

            UI.CursorMode = CursorMode.Shown;
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
            if (Input.IsPressed(Key.Escape))
            {
                Paused = !Paused;

                if (Paused)
                    _pauseRoot.Open(_pauseMenu);
                else
                    _pauseRoot.Close();
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
            UI.AddRoot(_pauseRoot);
            _pauseMenu = new PauseMenu();

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
            //CurrentScene.GetSystem<BackgroundSystem>().CurrentBackground = new TestBackground();
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            Console.WriteLine("Content loaded");

            //Temporary
            var settings = new InputSettings(Content.DefaultInputSettings);
            var profile = new InputProfile(settings);

            Input.Profile = profile;
            profile.Initialize();

            _stateMachine.ForceState(GlobalState.Game);
        }
    }
}
