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
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame);
        }

        protected override void Update(float deltaTime)
        {
            _stateMachine.Update(deltaTime);
        }

        protected override void Draw()
        {
            _stateMachine.Draw();
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

        }

        Layer l;
        Sprite s;

        private void DrawGame()
        {
            Graphics.BeginLayer(l);

            s.Draw(l.MousePosition);

            Graphics.EndCurrentLayer();
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            Console.WriteLine("Content loaded");
            _stateMachine.ForceState(GlobalState.Game);

            l = new Layer(320, 180, 1f);
            s = new Sprite("burn");
        }
    }
}
