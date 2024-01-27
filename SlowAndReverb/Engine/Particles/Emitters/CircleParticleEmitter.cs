using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class CircleParticleEmitter : ShapeParticleEmitter
    {
        private readonly float _radius;

        public CircleParticleEmitter(ParticleBehaviour behaviour, float radius) : base(behaviour)
        {
            _radius = radius;
        }

        protected override Vector2 GeneratePositionInsideShape()
        {
            return GeneratePosition(_radius * Random.NextFloat());
        }

        protected override Vector2 GeneratePositionAroundBorder()
        {
            return GeneratePosition(_radius);
        }

        private Vector2 GeneratePosition(float radius)
        {
            Vector2 result = Position + new Vector2(radius, 0f);
            float angle = Maths.TwoPI * Random.NextFloat();

            return result.Rotate(Position, angle);
        }
    }
}
