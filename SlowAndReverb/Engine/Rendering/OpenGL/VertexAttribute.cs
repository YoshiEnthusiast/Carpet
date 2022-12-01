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
        // Тут будет больше таких заготовок
        public static readonly VertexAttribute Vec2 = new VertexAttribute(VertexAttribPointerType.Float, sizeof(float), 2);

        private readonly VertexAttribPointerType _type;
        private readonly int _size;
        private readonly int _count;

        public VertexAttribute(VertexAttribPointerType type, int size, int count)
        {
            _type = type;   
            _size = size;
            _count = count;
        }

        public VertexAttribPointerType Type => _type;
        public int Size => _size;
        public int Count => _count;
    }
}
