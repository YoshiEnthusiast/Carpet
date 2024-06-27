namespace Carpet
{
    public struct Std140LayoutItem
    {
        private const int ScalarSize = 4;
        private const int Vector4Size = ScalarSize * 4;

        private Std140LayoutItem(int size)
        {
            BaseAlignment = size;
        }

        public int BaseAlignment { get; private init; }

        public static Std140LayoutItem Scalar()
        {
            return new Std140LayoutItem(ScalarSize);
        }

        public static Std140LayoutItem Vector2()
        {
            return new Std140LayoutItem(ScalarSize * 2);
        }

        public static Std140LayoutItem Vector3()
        {
            return new Std140LayoutItem(ScalarSize * 4);
        }

        public static Std140LayoutItem Vector4()
        {
            return new Std140LayoutItem(Vector4Size);
        }

        public static Std140LayoutItem Array(int length)
        {
            return new Std140LayoutItem(Vector4Size * length);
        }

        public static Std140LayoutItem Matrix(int culomns)
        {
            return new Std140LayoutItem(Vector4Size * culomns);
        }
    }
}
