namespace Carpet.Examples.RayCasting
{
    public class CircleOccluder : Entity
    {
        private const float Offset = 8f;

        private readonly SineWave _wave;
        private readonly float _initialY;

        public CircleOccluder(float x, float y) : base(x, y)
        {
            Add(new Sprite("circleOccluder"));

            Add(new LightOccluder()
            {
                Mode = OcclusionMode.SpriteComponent
            });

            _wave = Add(new SineWave()
            {
                Increment = 0.05f
            });

            _initialY = y;
        }

        protected override void Update(float deltaTime)
        {
            Y = _initialY + Offset * _wave.Value;
        }
    }
}
