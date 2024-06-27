using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public sealed class Pipeline
    {
        private readonly List<RenderPass> _passes = [];

        public RenderPass CurrentPass { get; private set; }

        public T AddPass<T>(T pass) where T : RenderPass
        {
            _passes.Add(pass);

            return pass;
        }

        public void RemovePass(RenderPass pass)
        {
            _passes.Remove(pass);
        }

        public RenderPass GetPass(int index)
        {
            return _passes[index];
        }

        public T GetPass<T>() where T : RenderPass
        {
            foreach (RenderPass pass in _passes)
                if (pass is T result)
                    return result;

            return default;
        }

        public void Process()
        {
            foreach (RenderPass pass in _passes)
            {
                CurrentPass = pass;
                pass.Process();
            }

            CurrentPass = null;
        }
    }
}
