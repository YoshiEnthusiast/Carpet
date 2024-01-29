namespace Carpet
{
    public struct ParticleData
    {
        public ParticleData(ParticleBehavior behaviour)
        {
            Behaviour = behaviour;
        }

        public ParticleBehavior Behaviour { get; private init; }
        public Entity Follow { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Scale { get; set; }

        public float LifeDuration { get; set; }
    }
}
