namespace Carpet
{
    public sealed class Accumulator
    {
        private float _x;
        private float _y;

        public void Charge(Vector2 value)
        {
            ChargeX(value.X);
            ChargeY(value.Y);
        }

        public Vector2 Release()
        {
            return new Vector2(ReleaseX(), ReleaseY());
        }

        public void ChargeX(float value)
        {
            _x += value;
        }

        public int ReleaseX()
        {
            int value = Maths.Floor(_x);
            _x -= value;

            return value;
        }

        public void ChargeY(float value)
        {
            _y += value;
        }

        public int ReleaseY()
        {
            int value = Maths.Floor(_y);
            _y -= value;

            return value;
        }
    }
}
