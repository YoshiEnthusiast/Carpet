using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class State : IState
    {
        private readonly Action<float> _onUpdate;

        private readonly Action _onDraw;
        private readonly Action _onStart;
        private readonly Action _onTerminate;

        public State(Action<float> onUpdate, Action onDraw, Action onStart, Action onTerminate)
        {
            _onUpdate = onUpdate;
            _onDraw = onDraw;
            _onStart = onStart;
            _onTerminate = onTerminate;
        }

        public State(Action<float> onUpdate, Action onDraw) : this(onUpdate, onDraw, null, null)
        {

        }

        public void Update(float deltaTime)
        {
            _onUpdate?.Invoke(deltaTime);
        }

        public void Draw()
        {
            _onDraw?.Invoke();
        }

        public void OnStart()
        {
            _onStart?.Invoke();
        }

        public void OnTerminate()
        {
            _onTerminate?.Invoke();
        }
    }
}
