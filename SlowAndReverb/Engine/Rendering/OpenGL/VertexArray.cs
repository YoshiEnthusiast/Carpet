using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;

namespace SlowAndReverb
{
    public sealed class VertexArray<T> : OpenGLObject where T : struct
    {
        public VertexArray(VertexBuffer<T> vertexBuffer, ElementBuffer elementBuffer, VertexAttribute[] attributes)
        {
            VertexBuffer = vertexBuffer;
            ElementBuffer = elementBuffer;

            GL.CreateVertexArrays(1, out int handle);
            Handle = handle;

            Bind();
            VertexBuffer.Bind();
            ElementBuffer.Bind();

            int stride = attributes.Sum(attribute => attribute.GetSize());
            int offset = 0;

            for (int i = 0; i < attributes.Length; i++)
            {
                VertexAttribute attribute = attributes[i];

                GL.VertexAttribPointer(i, attribute.Count, attribute.Type, false, stride, offset);
                GL.EnableVertexAttribArray(i);

                offset += attribute.GetSize();
            }
        }

        public VertexBuffer<T> VertexBuffer { get; private init; }
        public ElementBuffer ElementBuffer { get; private init; }

        public void Draw(PrimitiveType type)
        {
            Bind();
            ElementBuffer.Draw(type);
        }

        protected override void Bind(int handle)
        {
            GL.BindVertexArray(handle);
        }
    }
}
