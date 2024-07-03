using System.Collections.Generic;

namespace Carpet
{
    public sealed class Pipeline
    {
        private readonly List<Pass> _passes = [];

        public Pass CurrentPass { get; private set; }

        public T AddPass<T>(T pass) where T : Pass
        {
            _passes.Add(pass);

            return pass;
        }

        public void RemovePass(Pass pass)
        {
            _passes.Remove(pass);
        }

        public Pass GetPass(int index)
        {
            return _passes[index];
        }

        public T GetPass<T>() where T : Pass
        {
            foreach (Pass pass in _passes)
                if (pass is T result)
                    return result;

            return default;
        }

        public void Process()
        {
            foreach (Pass pass in _passes)
            {
                CurrentPass = pass;
                pass.Process();
            }

            CurrentPass = null;
        }
    }
}
