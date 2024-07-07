namespace Carpet.Examples.Platforming
{
    public abstract class AutoTile : Entity
    {
        private readonly Sprite _sprite;
        private readonly Dictionary<Vector2, AutoTile> _neighbors = [];

        public AutoTile(string tileSet, float x, float y) : base(x, y)
        {
            Size = new Vector2(8f);

            Type = GetType();
            TileSet = tileSet;

            _sprite = Add(new Sprite(tileSet, (int)Width, (int)Height)
            {
                Depth = Depths.Blocks
            });
        }

        public BlockGroup Group { get; set; }

        public bool NeedsRefresh { get; set; } = true;

        public Type Type { get; private init; }
        public string TileSet { get; private init; }

        protected override void Update(float deltaTime)
        {
            if (NeedsRefresh)
            {
                Refresh();

                NeedsRefresh = false;
            }
        }

        private void Refresh()
        {
            _neighbors.Clear();

            int mask = 0;
            int index = 0;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    index++;

                    if (FindNeighbor(x, y) is null)
                        continue;

                    if (x != 0 && y != 0 && (FindNeighbor(x, 0) is null || FindNeighbor(0, y) is null))
                        continue;

                    mask += 1 << index - 1;
                }
            }

            _sprite.Frame = Content.GetTileFrame(mask);
        }

        public AutoTile GetNeighbor(int x, int y)
        {
            if (_neighbors.TryGetValue(new Vector2(x, y), out AutoTile neighbor))
                return neighbor;

            return null;
        }

        private AutoTile FindNeighbor(int x, int y)
        {
            AutoTile neighbor = GetNeighbor(x, y);

            if (neighbor is not null)
                return neighbor;

            var offset = new Vector2(x, y);
            neighbor = Scene.CheckPosition<AutoTile>(Position + offset * Size);

            if (neighbor is not null && neighbor.Type == Type)
            {
                _neighbors[offset] = neighbor;
                
                return neighbor;
            }

            return default;
        }
    }
}
