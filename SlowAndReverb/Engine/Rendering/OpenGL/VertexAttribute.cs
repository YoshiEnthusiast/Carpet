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
        public static readonly VertexAttribute Vec3 = new VertexAttribute(VertexAttribPointerType.Float, sizeof(float), 3);
        public static readonly VertexAttribute Vec4 = new VertexAttribute(VertexAttribPointerType.Float, sizeof(float), 4);

        private readonly int _elementSize;

        private VertexAttribute(VertexAttribPointerType type, int elementSize, int count)
        {
            Type = type;   
            Count = count;

            _elementSize = elementSize;
        }

        public VertexAttribPointerType Type { get; private init; }
        public int Count { get; private init; }

        public int GetSize()
        {
            return _elementSize * Count;
        }
    }
}
