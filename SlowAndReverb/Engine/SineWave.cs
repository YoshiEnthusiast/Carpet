namespace Carpet
{
    public class SineWave : Component
    {
        private float _x;

        public SineWave(float start)
        {
            _x = start;
        }

        public SineWave()
        {

        }

        public float Increment { get; set; } = 0.1f;
        public float Value { get; private set; }

        public float Normalized => (Value + 1f) / 2f;

        protected override void Update(float deltaTime)
        {
            _x += Increment * deltaTime;

            UpdateValue();
        }

        private void UpdateValue()
        {
            Value = Maths.Sin(_x);
        }
    }
}
