namespace SlowAndReverb
{
    public abstract class ShapeParticleEmitter : ParticleEmitter
    {
        public ShapeParticleEmitter(ParticleBehaviour behaviour) : base(behaviour)
        {

        }

        public ParticleShapeBehaviour ShapeBehaviour { get; set; }

        protected override Vector2 GeneratePosition()
        {
            if (ShapeBehaviour == ParticleShapeBehaviour.Fill)
                return GeneratePositionInsideShape();

            return GeneratePositionAroundBorder();
        }

        protected abstract Vector2 GeneratePositionInsideShape();
        protected abstract Vector2 GeneratePositionAroundBorder();
    }
}
