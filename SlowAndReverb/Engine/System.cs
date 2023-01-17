using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class System
    {
        public System(Scene scene)
        {
            Scene = scene;
        }

        public Scene Scene { get; private init; }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void OnBeforeDraw()
        {

        }

        public virtual void Draw()
        {

        }
    }
}
