using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class ParticleSystem : System
    {
        private readonly Particle[] _particles;
        private int _particlesCount;

        public ParticleSystem(Scene scene, int maxParticles) : base(scene)
        {
            _particles = new Particle[maxParticles];
        }

        public void Add(ParticleData data)
        {
            ref Particle particle = ref _particles[_particlesCount];

            particle.Active = true;
            particle.Behavior = data.Behavior;
            
            particle.Follow = data.Follow;
            particle.Position = data.Position;
            particle.Velocity = data.Velocity;
            particle.InitialScale = data.Scale;
            particle.LifeDuration = data.LifeDuration;

            particle.Life = 0f;

            _particlesCount = (_particlesCount + 1) % _particles.Length;
        }

        public override void Update(float deltaTime)
        {
            for (int i = _particles.Length - 1; i >= 0; i--)
            {
                ref Particle particle = ref _particles[i];

                if (!particle.Active)
                    continue;

                particle.Behavior.Update(ref particle, deltaTime);
            }
        }

        public override void Draw()
        {
            for (int i = _particles.Length - 1; i >= 0; i--)
            {
                ref Particle particle = ref _particles[i];

                if (!particle.Active)
                    continue;

                particle.Behavior.Draw(ref particle);
            }
        }
    }
}
