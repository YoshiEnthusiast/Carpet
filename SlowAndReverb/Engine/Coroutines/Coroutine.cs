using System;
using System.Collections;

namespace SlowAndReverb
{
    public sealed class Coroutine
    {
        private readonly IEnumerator _enumerator;

        private float _delay;
        private bool _finished;

        public Coroutine(IEnumerator enumerator, float initialDelay)
        {
            _enumerator = enumerator;

            _delay = initialDelay;
        }

        public Coroutine(IEnumerator enumerator) : this(enumerator, 0f)
        {

        }

        public bool Finished => _finished;

        public void Update(float deltaTime)
        {
            if (_finished)
                return;

            while (deltaTime > 0f)
            {
                float time = Math.Min(deltaTime, _delay);

                _delay -= time;

                if (_delay <= 0f && !TakeStep(_enumerator))
                {
                    _finished = true;

                    return;
                }

                deltaTime -= time;
            }
        }

        public void Stop()
        {
            _finished = true;
        }

        private bool TakeStep(IEnumerator enumerator)
        {
            if (enumerator.Current is IEnumerator child && TakeStep(child))
            {
                _delay = 0f;

                return true;
            }

            if (!enumerator.MoveNext())
                return false;

            object current = enumerator.Current;

            if (current is int || current is float)
                _delay = (float)current;

            return true;
        }
    }
}
