namespace SlowAndReverb
{
    public struct ParticleData
    {
        public ParticleData(ParticleBehaviour behaviour)
        {
            Behaviour = behaviour;
        }

        public ParticleBehaviour Behaviour { get; private init; }
        public Entity Follow { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Scale { get; set; }

        public float LifeDuration { get; set; }
    }
}
