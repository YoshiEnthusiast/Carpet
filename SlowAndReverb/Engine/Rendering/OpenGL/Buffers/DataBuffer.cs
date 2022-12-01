using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;

namespace SlowAndReverb
{
    public abstract class DataBuffer<T> : OpenGLObject where T : struct
    {
        private readonly int _size;

        public DataBuffer()
        {
            GL.CreateBuffers(1, out int handle);

            Handle = handle;
            _size = Unsafe.SizeOf<T>();
        }

        protected BufferTarget BufferTarget { get; init; }

        public virtual void SetData(T[] data)
        {
            Bind();
            GL.BufferData(BufferTarget, _size * data.Length, data, BufferUsageHint.StaticDraw);
        }

        protected override void Bind(int handle)
        {
            GL.BindBuffer(BufferTarget, handle);
        }
    }
}
