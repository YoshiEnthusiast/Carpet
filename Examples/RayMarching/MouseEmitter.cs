namespace Carpet.Examples.RayMarching
{
    public class MouseEmitter : Entity
    {
        public MouseEmitter(float x, float y) : base(x, y)
        {
            Add(new RayEmitter()
            {
                Mode = OcclusionMode.Circle,
                CircleRadius = 30,
                Color = Color.BlueViolet
            });

            Size = new Vector2(50f);
        }

        protected override void Update(float deltaTime)
        {
            Position = Game.MainLayer.MousePosition;
        }
    }
}
