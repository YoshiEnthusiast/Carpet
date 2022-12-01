using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public sealed class ElementBuffer : DataBuffer<uint>
    {
        private int _count;

        public ElementBuffer()
        {
            BufferTarget = BufferTarget.ElementArrayBuffer;
        }

        public override void SetData(uint[] indices)
        {
            _count = indices.Length;   

            base.SetData(indices);
        }

        public void Draw(PrimitiveType type)
        {
            GL.DrawElements(type, _count, DrawElementsType.UnsignedInt, 0);
        }
    }
}
