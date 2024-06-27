using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class OffCenterParticleBehavior : ParticleBehavior
    {
        public override ParticleData Create(Vector2 position, Vector2 sourcePosition)
        {
            ParticleData data = base.Create(position, sourcePosition);
            float angle = Maths.Atan2(position, sourcePosition);

            data.Velocity = -data.Velocity.Rotate(angle);

            return data;
        }
    }
}
