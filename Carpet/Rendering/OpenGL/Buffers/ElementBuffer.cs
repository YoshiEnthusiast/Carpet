using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public sealed class ElementBuffer : DataBuffer<uint>
    {
        private int _count;

        public ElementBuffer()
        {
            BufferTarget = BufferTarget.ElementArrayBuffer;
        }

        public override void SetData(int length, uint[] indices)
        {
            _count = length;

            base.SetData(length, indices);
        }

        public void Draw(PrimitiveType type)
        {
            GL.DrawElements(type, _count, DrawElementsType.UnsignedInt, 0);
        }
    }
}
