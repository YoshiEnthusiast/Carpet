namespace Carpet.Examples.RayMarching
{
    public class Occluder : Entity
    {
        private Sprite _sprite;

        public Occluder(float x, float y) : base(x, y)
        {

        }

        public float Scale { get; set; } = 1f;
        public float SpeedMultiplier { get; set; } = 0.01f;

        protected override void OnAdded()
        {
            Add(new RayOccluder()
            {
                Mode = OcclusionMode.SpriteComponent,
            });

            _sprite = Add(new Sprite("occluder")
            {
                Scale = new Vector2(Scale)
            });
        }

        protected override void Update(float deltaTime)
        {
            _sprite.Angle += deltaTime * SpeedMultiplier;
        }
    }
}
