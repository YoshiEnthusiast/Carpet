namespace Carpet.Examples.RayCasting
{
    public class RectangleOccluder : Entity
    {
        public RectangleOccluder(float x, float y) : base(x, y)
        {
            Add(new Sprite("rectangleOccluder"));

            Add(new LightOccluder()
            {
                Mode = OcclusionMode.SpriteComponent
            });
        }
    }
}
