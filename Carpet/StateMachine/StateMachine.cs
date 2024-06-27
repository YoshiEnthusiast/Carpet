using System;
using System.Collections.Generic;

namespace Carpet
{
    public class StateMachine<T> : Component where T : struct
    {
        private readonly Dictionary<T, IState> _states = [];

        public T? StateID { get; private set; }
        public T? PreviousStateID { get; private set; }

        private IState CurrentState
        {
            get
            {
                if (StateID is null)
                    return null;

                return _states[StateID.Value];
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
            if (Equals(StateID, id))
                return;

            CurrentState?.OnTerminate();

            PreviousStateID = StateID;
            StateID = id;

            CurrentState?.OnStart();
        }

        protected override void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }

        protected override void Draw()
        {
            CurrentState?.Draw();
        }
    }
}
