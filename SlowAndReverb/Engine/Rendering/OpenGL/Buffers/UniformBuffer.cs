using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public sealed class UniformBuffer : OpenGLObject
    {
        private const int MaxBaseAlignment = 16;

        private Std140LayoutItem[] _items;
        
        public UniformBuffer()
        {
            GL.CreateBuffers(1, out int handle);

            Handle = handle;
        }

        public void Initialize(IEnumerable<Std140LayoutItem> items)
        {
            _items = items.ToArray();

            int size = GetAlignedOffset(_items.Length - 1);

            if (_items.Length > 0)
                size += _items[0].BaseAlignment;

            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, size, 0, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, Handle);
        }

        public override void Bind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
        }

        public void SetValue<T>(T value, int index) where T : struct
        {
            Std140LayoutItem item = _items[index];

            int baseAlignment = item.BaseAlignment;
            int alignedOffset = GetAlignedOffset(index);

            GL.BufferSubData(BufferTarget.UniformBuffer, alignedOffset, baseAlignment, ref value);
        }

        private int GetAlignedOffset(int index)
        {
            int alignedOffset = 0;
            int lastBaseAlignment = 0;

            for (int i = 0; i < index + 1; i++)
            {
                Std140LayoutItem item = _items[i];

                int baseAlignment = item.BaseAlignment;
                int roundTo = Math.Min(baseAlignment, MaxBaseAlignment);
                alignedOffset += lastBaseAlignment;

                alignedOffset = Maths.Ceiling(alignedOffset / (float)roundTo) * roundTo;

                lastBaseAlignment = baseAlignment;
            }

            return alignedOffset;
        }
    }
}
