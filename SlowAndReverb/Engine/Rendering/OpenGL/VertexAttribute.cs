using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public struct VertexAttribute
    {
        public static readonly VertexAttribute Float = new VertexAttribute(VertexAttribPointerType.Float, sizeof(float), 1);
        public static readonly VertexAttribute Vec2 = new VertexAttribute(VertexAttribPointerType.Float, sizeof(float), 2);
        public static readonly VertexAttribute Vec4 = new VertexAttribute(VertexAttribPointerType.Float, sizeof(float), 4);

        private readonly VertexAttribPointerType _type;
        private readonly int _elementSize;
        private readonly int _count;

        public VertexAttribute(VertexAttribPointerType type, int elementSize, int count)
        {
            _type = type;   
            _elementSize = elementSize;
            _count = count;
        }

        public VertexAttribPointerType Type => _type;
        public int Count => _count;

        public int GetSize()
        {
            return _elementSize * _count;
        }
    }
}
