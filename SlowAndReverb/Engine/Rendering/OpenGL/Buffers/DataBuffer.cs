using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.CompilerServices;

namespace Carpet
{
    public abstract class DataBuffer<T> : OpenGLObject where T : struct
    {
        private readonly int _itemSize;

        public DataBuffer()
        {
            GL.CreateBuffers(1, out int handle);

            Handle = handle;
            _itemSize = Unsafe.SizeOf<T>();
        }

        protected BufferTarget BufferTarget { get; init; }

        public virtual void Initialize(int itemsCount)
        {
            int size = _itemSize * itemsCount;

            Bind();
            GL.BufferData(BufferTarget, size, (T[])null, BufferUsageHint.DynamicDraw);
        }

        public virtual void SetData(int length, T[] data)
        {
            Bind();
            GL.BufferSubData(BufferTarget, 0, _itemSize * length, data);
        }

        protected override void Bind(int handle)
        {
            GL.BindBuffer(BufferTarget, handle);
        }
    }
}
