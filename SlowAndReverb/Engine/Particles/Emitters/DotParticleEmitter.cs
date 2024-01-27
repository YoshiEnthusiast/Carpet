using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class DotParticleEmitter : ParticleEmitter
    {
        public DotParticleEmitter(ParticleBehaviour behaviour) : base(behaviour)
        {

        }

        protected override Vector2 GeneratePosition()
        {
            return Position;
        }
    }
}
