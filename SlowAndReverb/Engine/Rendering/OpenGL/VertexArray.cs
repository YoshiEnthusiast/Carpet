using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;

namespace SlowAndReverb
{
    public sealed class VertexArray<T> : OpenGLObject where T : struct
    {
        private readonly VertexBuffer<T> _vertexBuffer;
        private readonly ElementBuffer _elementBuffer;

        public VertexArray(VertexBuffer<T> vertexBuffer, ElementBuffer elementBuffer, VertexAttribute[] attributes)
        {
            _vertexBuffer = vertexBuffer;
            _elementBuffer = elementBuffer;

            GL.CreateVertexArrays(1, out int handle);
            Handle = handle;

            Bind();
            _vertexBuffer.Bind();
            _elementBuffer.Bind();

            int stride = attributes.Sum(attribute => GetVertexAttributeSize(attribute));
            int offset = 0;

            for (int i = 0; i < attributes.Length; i++)
            {
                VertexAttribute attribute = attributes[i];

                GL.VertexAttribPointer(i, attribute.Count, attribute.Type, false, stride, offset);
                GL.EnableVertexAttribArray(i);

                offset += GetVertexAttributeSize(attribute);
            }
        }

        public VertexBuffer<T> VertexBuffer => _vertexBuffer;

        public void Draw(PrimitiveType type)
        {
            Bind();
            _elementBuffer.Draw(type);
        }

        protected override void Bind(int handle)
        {
            GL.BindVertexArray(handle);
        }

        private int GetVertexAttributeSize(VertexAttribute attribute)
        {
            return attribute.Size * attribute.Count;
        }
    }
}
