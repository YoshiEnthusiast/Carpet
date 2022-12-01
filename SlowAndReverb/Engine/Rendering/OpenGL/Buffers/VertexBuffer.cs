using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public sealed class VertexBuffer<T> : DataBuffer<T> where T : struct
    {
        public VertexBuffer()
        {
            BufferTarget = BufferTarget.ArrayBuffer;
        }
    }
}
