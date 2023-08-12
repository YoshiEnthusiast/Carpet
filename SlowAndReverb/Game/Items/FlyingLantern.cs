using System.Collections;

namespace SlowAndReverb
{
    public class FlyingLantern : Entity
    {
        private readonly SineWave _wave;
        private readonly Light _light;
        private readonly CoroutineRunner _runner;

        private readonly Color _lightColor = new Color(237, 154, 31);
        private readonly float _maxLightRadius = 50f;

        private readonly Range _shimmerIntervalRange = new Range(10f, 15f);
        private readonly Range _lightIntensityRange = new Range(0.85f, 1f);

        private readonly float _initialY;
        private readonly float _yOffset = 2f;

        public FlyingLantern(float x, float y) : base(x, y)
        {
            _wave = Add(new SineWave()
            {
                Increment = 0.05f
            });

            _light = Add(new Light());

            _runner = Add(new CoroutineRunner());

            Add(new Sprite("flyingLantern"));

            _initialY = y;
        }

        protected override void Update(float deltaTime)
        {
            Y = _initialY + _yOffset * _wave.Value;
        }

        protected override void OnAdded()
        {
            _runner.StartCoroutine(UpdateLightShimmer());
        }

        private IEnumerator UpdateLightShimmer()
        {
            while (true)
            {
                float value = Random.NextFloat(_lightIntensityRange);

                _light.Radius = _maxLightRadius * value;
                _light.Color = _lightColor * 0f;

                yield return Random.NextFloat(_shimmerIntervalRange);
            }
        }
    }
}
