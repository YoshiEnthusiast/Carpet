using System.Collections;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public sealed class CoroutineRunner
    {
        private Dictionary<int, Coroutine> _coroutines = new Dictionary<int, Coroutine>();
        private int _totalCoroutinesAdded;

        public int AliveCount => _coroutines.Count;

        public void Update()
        {
            foreach (int id in _coroutines.Keys)
            {
                Coroutine coroutine = _coroutines[id];

                coroutine.Update();

                if (coroutine.Finished)
                    _coroutines.Remove(id);
            }
        }

        public int StartCoroutine(IEnumerator enumerator, float initialDelay)
        {
            int id = _totalCoroutinesAdded;
            var coroutine = new Coroutine(enumerator, initialDelay);
            
            _coroutines.Add(id, coroutine);
            _totalCoroutinesAdded++;

            return id;
        }

        public int StartCoroutine(IEnumerator enumerator)
        {
            return StartCoroutine(enumerator, 0f);
        }

        public bool StopCoroutine(int id)
        {
            return _coroutines.Remove(id);
        }

        public void StopAllCoroutines()
        {
            _coroutines.Clear();
        }

        public bool IsAlive(int id)
        {
            return _coroutines.ContainsKey(id);
        }
    }
}
