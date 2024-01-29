using System;

namespace Carpet
{
    public abstract class ParticleEmitter : Component
    {
        private ParticleSystem _system;
        private float _timer;

        public ParticleEmitter(ParticleBehavior behaviour)
        {
            Behaviour = behaviour;
        }

        public ParticleBehavior Behaviour { get; set; }

        public int EmitCount { get; set; } = 1;
        public int EmitCountVariation { get; set; }
        public float Interval { get; set; } = 10f;
        public float IntervalVariation { get; set; }

        public void Emit()
        {
            int count = EmitCount + Random.NextInt(-EmitCountVariation, EmitCountVariation);

            for (int i = 0; i < count; i++)
                EmitOne();
        }

        public void EmitOne(Vector2 position)
        {
            ParticleData data = Behaviour.Create(position, Position);

            _system.Add(data);
        }

        public void EmitOne()
        {
            EmitOne(GeneratePosition());
        }

        protected abstract Vector2 GeneratePosition();

        protected override void Update(float deltaTime)
        {
            _timer -= deltaTime;

            if (_timer <= 0f)
            {
                Emit();

                _timer = Interval + Random.NextFloat(-IntervalVariation, IntervalVariation);
            }
        }

        protected override void OnAdded()
        {
            _system = Scene.GetSystem<ParticleSystem>();
        }
    }
}
