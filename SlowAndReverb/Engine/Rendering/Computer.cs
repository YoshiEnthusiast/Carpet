using System;
using System.Collections.Generic;

namespace Carpet
{
    public abstract class Computer : ShaderProgramWrapper
    {
        private readonly List<ShaderStorageBuffer> _buffers = new List<ShaderStorageBuffer>();

        private ShaderStorageBuffer _boundBuffer;

        public ComputeShaderProgram ShaderProgram { get; protected init; }
        protected override ShaderProgram Program => ShaderProgram;

        public void Compute(int width, int height, int depth)
        {
            for (int i = 0; i < _buffers.Count; i++)
            {
                ShaderStorageBuffer buffer = _buffers[i];

                buffer.Bind();
                buffer.BindToTarget(i);
            }

            ShaderProgram.Bind();

            SetUniforms();

            ShaderProgram.Compute(width, height, depth);
        }

        public int PushBuffer(ReadOnlySpan<ComputeItem> items)
        {
            var buffer = new ShaderStorageBuffer(items);
            _buffers.Add(buffer);

            return _buffers.Count - 1;
        }

        public void BindBuffer(int index)
        {
            ShaderStorageBuffer buffer = _buffers[index];

            buffer.Bind();
            _boundBuffer = buffer;
        }

        public void Free()
        {
            for (int i = 0; i < _buffers.Count; i++)
            {
                ShaderStorageBuffer buffer = _buffers[i];

                buffer.Delete();
            }
        }

        public void SetItem<T>(int index, T value) where T : struct
        {
            _boundBuffer.SetValue(index, value);
        }

        public void SetItem<T>(int index, T[] value) where T : struct
        {
            _boundBuffer.SetValue(index, value);
        }

        public void GetItem<T>(int index, ref T value) where T : struct
        {
            _boundBuffer.GetValue(index, ref value);
        }

        public void GetItem<T>(int index, T[] value) where T : struct
        {
            _boundBuffer.GetValue(index, value);
        }
    }
}
