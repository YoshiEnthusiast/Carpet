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

        private UniformBlockItem(int size)
        {
            BaseAlignment = size;
        }

        public int BaseAlignment { get; private init; }

        public static UniformBlockItem Scalar()
        {
            return new UniformBlockItem(ScalarSize);
        }

        public static UniformBlockItem Vector(int scalarCount)
        {
            return new UniformBlockItem(ScalarSize * scalarCount);
        }

        public static UniformBlockItem ArrayElementOrMatrixCulomn()
        {
            return new UniformBlockItem(ScalarSize * 4);
        }
    }
}
