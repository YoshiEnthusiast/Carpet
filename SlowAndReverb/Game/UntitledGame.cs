using System;
using System.Threading;

namespace SlowAndReverb
{
    public sealed class UntitledGame : Engine
    {
        private readonly StateMachine<GlobalState> _stateMachine = new StateMachine<GlobalState>();

        private Thread _contentLoadingThread;

        public UntitledGame(double updateFrequence, double drawFrequency, string name) : base(updateFrequence, drawFrequency, name)
        {
            _stateMachine.SetState(GlobalState.Loading, UpdateContentLoading, null);
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame, StartGame, null);
        }

        public static Scene CurrentScene { get; set; }

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
            RenderTargets.Initialize();
            Layers.Initialize();
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
            CurrentScene.Update(deltaTime);
        }

        private void DrawGame()
        {
            CurrentScene.Draw();
        }

        private void StartGame()
        {
            CurrentScene = new Scene();

            CurrentScene.Add(new TestEntity(0f, 0f));
            //Scene.Add(new TestEntity2(180f, 90f));
            //Scene.Add(new Platform(120f, 100f));

            for (int i = 0; i < 25; i++)
                CurrentScene.Add(new DefaultBlock(68f + i * 8f, 100)); // 148

            //Scene.Add(new Block("tileset", 68f, 140f));
            //Scene.Add(new Block("tileset", 68f + 8f * 25f, 140f));

            CurrentScene.Add(new Player(100f, 0f));

            CurrentScene.Add(new FlyingLantern(80f, 80f));

            CurrentScene.Add(new Coin(150f, 60f));
            CurrentScene.Add(new Coin(230f, 80f));

            CurrentScene.Color = new Color(100, 100, 100);
            Engine.DebugLighting = false;

            CurrentScene.Initialize();
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
