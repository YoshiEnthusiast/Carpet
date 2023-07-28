using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public struct UniformBlockItem
    {
        private const int ScalarSize = 4;
        private const int Vector4Size = ScalarSize * 4;

        private UniformBlockItem(int size)
        {
            BaseAlignment = size;
        }

        public int BaseAlignment { get; private init; }

        public static UniformBlockItem Scalar()
        {
            return new UniformBlockItem(ScalarSize);
        }

        public static UniformBlockItem Vector2()
        {
            return new UniformBlockItem(ScalarSize * 2);
        }

        {
            return new UniformBlockItem(ScalarSize * 4);
        }

        public static UniformBlockItem Vector4()
        {
            return new UniformBlockItem(Vector4Size);
        }

        public static UniformBlockItem Array(int length)
        {
            return new UniformBlockItem(Vector4Size * length);
        }

        public static UniformBlockItem Matrix(int culomns)
        {
            return new UniformBlockItem(Vector4Size * culomns);
        }
    }
}
