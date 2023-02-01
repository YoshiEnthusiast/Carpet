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

        protected override void Update(float deltaTime)
        {
            _stateMachine.Update(deltaTime);
        }

        protected override void Draw()
        {
            _stateMachine.Draw();
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
            Scene.Current?.Update(deltaTime);
        }

        private void DrawGame()
        {
            Scene.Current?.Draw();
        }

        private void StartGame()
        {
            Scene.Current = new Scene();

            Scene.Add(new TestEntity(0f, 0f));
            //Scene.Add(new TestEntity2(180f, 90f));
            //Scene.Add(new Platform(120f, 100f));

            for (int i = 0; i < 25; i++)
                Scene.Add(new Block("tileset", 68f + i * 8f, 148));

            Scene.Add(new Block("tileset", 68f, 140f));
            Scene.Add(new Block("tileset", 68f + 8f * 25f, 140f));

            Scene.Add(new Player(100f, 120f));

            Scene.Current.Brightness = 1f;
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            Console.WriteLine("Content loaded");
            _stateMachine.ForceState(GlobalState.Game);
        }
    }
}
