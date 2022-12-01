using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class OpenGLObject
    {
        public int Handle { get; protected init; }

        public virtual void Bind()
        {
            Bind(Handle);
        }

        public virtual void UnBind()
        {
            Bind(0);
        }

        protected virtual void Bind(int handle)
        {

        }
    }
}
