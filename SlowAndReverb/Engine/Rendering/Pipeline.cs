using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public sealed class Pipeline
    {
        private readonly List<PassBase> _passes = [];

        public PassBase CurrentPass { get; private set; }

        public T AddPass<T>(T pass) where T : PassBase
        {
            _passes.Add(pass);

            return pass;
        }

        public void RemovePass(PassBase pass)
        {
            _passes.Remove(pass);
        }

        public PassBase GetPass(int index)
        {
            return _passes[index];
        }

        public T GetPass<T>() where T : PassBase
        {
            foreach (PassBase pass in _passes)
                if (pass is T result)
                    return result;

            return default;
        }

        public void Process()
        {
            foreach (PassBase pass in _passes)
            {
                CurrentPass = pass;
                pass.Process();
            }

            CurrentPass = null;
        }
    }
}
