namespace Carpet.Examples.RayMarching
{
    public class Emitter : Entity
    {
        public Emitter(float x, float y) : base(x, y)
        {
            Size = new Vector2(50f);
        }
        
        public Color Color { get; set; }
        public int Radius { get; set; }
        public float Intensity { get; set; } = 1f;

        protected override void OnAdded()
        {
            Add(new RayEmitter()
            {
                Mode = OcclusionMode.Circle,
                CircleRadius = Radius,
                Color = Color,
                Intensity = Intensity
            });

        }
    }
}
