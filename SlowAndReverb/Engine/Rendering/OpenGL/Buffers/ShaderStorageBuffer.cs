using OpenTK.Graphics.OpenGL;
using System;
using System.ComponentModel.DataAnnotations;

namespace SlowAndReverb
{
    public sealed class ShaderStorageBuffer : OpenGLObject
    {
        private readonly Std430LayoutItem[] _items;

        public ShaderStorageBuffer(ReadOnlySpan<Std430LayoutItem> items)
        {
            GL.CreateBuffers(1, out int handle);

            Handle = handle;

            _items = items.ToArray();

            int length = _items.Length;
            int index = length - 1;

            int size = GetAlignedOffset(index);

            if (length > 0)
                size += _items[index].BaseAlignment;

            Bind();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, size, 0, BufferUsageHint.StaticDraw);
        }

        public void BindToTarget(int target)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, target, Handle);
        }

        public void SetValue<T>(int index, T value) where T : struct
        {
            Std430LayoutItem item = _items[index];

            int baseAlignment = item.BaseAlignment;
            int alignedOffset = GetAlignedOffset(index);

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, alignedOffset, baseAlignment, ref value);
        }

        public void SetValue<T>(int index, T[] value) where T : struct
        {
            Std430LayoutItem item = _items[index];

            int baseAlignment = item.BaseAlignment;
            int alignedOffset = GetAlignedOffset(index);

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, alignedOffset, baseAlignment, value);
        }

        public void GetValue<T>(int index, T[] buffer) where T : struct
        {
            Std430LayoutItem item = _items[index];

            int baseAlignment = item.BaseAlignment;
            int alignedOffset = GetAlignedOffset(index);

            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, alignedOffset, baseAlignment, buffer);
        }

        public void GetValue<T>(int index, ref T value) where T : struct
        {
            Std430LayoutItem item = _items[index];

            int baseAlignment = item.BaseAlignment;
            int alignedOffset = GetAlignedOffset(index);

            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, alignedOffset, baseAlignment, ref value);
        }

        protected override void Bind(int handle)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        }

        protected override void Delete(int handle)
        {
            GL.DeleteBuffer(handle);
        }

        private int GetAlignedOffset(int index)
        {
            int alignedOffset = 0;
            int lastBaseAlignment = 0;

            for (int i = 0; i < index + 1; i++)
            {
                Std430LayoutItem item = _items[i];

                int baseAlignment = item.BaseAlignment;
                int offset = item.AlignedOffset;
                alignedOffset += lastBaseAlignment;

                alignedOffset = Maths.RoundUp(alignedOffset, offset);

                lastBaseAlignment = baseAlignment;
            }

            return alignedOffset;
        }
    }
}
