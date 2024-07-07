namespace Carpet.Examples.Platforming
{
    public class AnchorBlock : Block
    {
        public AnchorBlock(float x, float y) : base("anchorTileset", x, y)
        {
            Add(new Anchor());
        }
    }
}
