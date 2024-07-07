using System.Collections;

namespace Carpet.Examples.RayCasting
{
    public class FlyingLantern : Entity
    {
        private const float Offset = 4f;

        private readonly SineWave _wave;
        private readonly Light _light;

        private readonly float _initialY;

        public FlyingLantern(float x, float y) : base(x, y)
        {
            _wave = Add(new SineWave()
            {
                Increment = 0.05f
            });

            Add(new Sprite("flyingLantern"));

            Add(new Light()
            {
                Color = new Color(237, 154, 31),
                Radius = 50f,
                Volume = 0.2f
            });

            _initialY = y;
        }

        protected override void Update(float deltaTime)
        {
            Y = _initialY + Offset * _wave.Value;
        }
    }
}
