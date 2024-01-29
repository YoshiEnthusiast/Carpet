namespace Carpet
{
    public static class Random
    {
        public const float EqualChance = 0.5f;

        private static readonly SystemRandom _generator = new();

        public static float NextFloat(float min, float max)
        {
            return min + (float)_generator.NextDouble() * (max - min);
        }

        public static float NextFloat(Range range)
        {
            return NextFloat(range.Min, range.Max);
        }

        public static float NextFloat(float max)
        {
            return NextFloat(0f, max);
        }

        public static float NextFloat()
        {
            return NextFloat(0f, 1f);
        }

        public static int NextInt(int min, int max)
        {
            return min + (int)(_generator.NextDouble() * (max - min));
        }

        public static int NexInt(RangeInt range)
        {
            return NextInt(range.Min, range.Max);
        }

        public static int NextInt(int max)
        {
            return NextInt(0, max);
        }

        public static Vector2 NextVector2(Vector2 min, Vector2 max)
        {
            float x = NextFloat(min.X, max.X);
            float y = NextFloat(min.Y, max.Y);

            return new Vector2(x, y);   
        }

        public static Vector2 NextVector2(Vector2 max)
        {
            return NextVector2(Vector2.Zero, max);
        }

        public static bool NextBool(float chance)
        {
            chance = Maths.Clamp(chance, 0f, 1f);

            return NextFloat() <= chance;
        }

        public static bool NextBool()
        {
            return NextBool(EqualChance);
        }

        public static float NextBinary(float chance)
        {
            if (NextBool(chance))
                return 1f;

            return 0f;
        }

        public static float NextBinary()
        {
            return NextBinary(EqualChance);
        }

        public static int NextBinaryInt(float chance)
        {
            if (NextBool(chance))
                return 1;

            return 0;
        }

        public static int NextBinaryInt()
        {
            return NextBinaryInt(EqualChance);
        }

        public static float NextSign(float chance)
        {
            if (NextBool(chance))
                return 1f;

            return -1f;
        }

        public static float NextSign()
        {
            return NextSign(EqualChance);
        }

        public static int NextSignInt(float chance)
        {
            if (NextBool(chance))
                return 1;

            return -1;
        }

        public static int NextSignInt()
        {
            return NextSignInt(EqualChance);
        }
    }
}
