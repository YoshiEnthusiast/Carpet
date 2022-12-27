using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public class StateMachine<T> where T : struct
    {
        private readonly Dictionary<T, IState> _states = new Dictionary<T, IState>();

        private T? _stateID;
        private T? _previousStateID;

        public T? CurrentStateID => _stateID;
        public T? PreviousStateID => _previousStateID;  

        private IState CurrentState
        {
            get
            {
                if (_stateID is null)
                    return null;

                return _states[_stateID.Value];
            }
        }

        public void SetState(T id, IState state)
        {
            if (_states.ContainsKey(id))
            {
                _states.Add(id, state);

                return;
            }

            _states[id] = state;
        }

        public void SetState(T id, Action<float> onUpdate, Action onDraw, Action onStart, Action onTerminate)
        {
            var state = new State(onUpdate, onDraw, onStart, onTerminate);

            SetState(id, state);
        }

        public void SetState(T id, Action<float> onUpdate, Action onDraw)
        {
            SetState(id, onUpdate, onDraw, null, null);
        }

        public void ForceState(T? id)
        {
            if (Equals(_stateID, id))
                return;

            CurrentState?.OnTerminate();

            _previousStateID = _stateID;
            _stateID = id;

            CurrentState?.OnStart();
        }

        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }

        public void Draw()
        {
            CurrentState?.Draw();
        }
    }
}
