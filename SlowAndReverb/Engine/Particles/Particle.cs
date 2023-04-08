using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public struct Particle
    {
        private Vector2 _position;

        public Vector2 Position
        {
            get
            {
                if (Follow is not null)
                    return _position + Follow.Position;

                return _position;
            }

            set
            {
                if (Follow is not null)
                {
                    _position = value - Follow.Position;

                    return;
                }    

                _position = value;
            }
        }

        public ParticleBehaviour Behaviour { get; set; }
        public Entity Follow { get; set; }

        public bool Active { get; set; }

        public Vector2 Velocity { get; set; }

        public Vector2 Scale { get; set; }
        public Vector2 InitialScale { get; set; }

        public Color Color { get; set; }

        public float Life { get; set; }
        public float LifeDuration { get; set; }
    }
}
