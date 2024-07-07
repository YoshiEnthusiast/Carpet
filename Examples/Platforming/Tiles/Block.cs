namespace Carpet.Examples.Platforming
{
    public abstract class Block : AutoTile
    {
        public Block(string tileSet, float x, float y) : base(tileSet, x, y)
        {
            Add(new SolidObject());
        }
    }
}
