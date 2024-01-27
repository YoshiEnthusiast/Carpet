using System;
using System.Collections.Generic;
using System.Numerics;

namespace Carpet
{
    public abstract class AutoTile : Entity
    {
        private readonly Sprite _sprite;
        private readonly Dictionary<Vector2, AutoTile> _neighbours = new Dictionary<Vector2, AutoTile>();

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
            _neighbours.Clear();

            int mask = 0;
            int index = 0;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    index++;

                    if (FindNeighbour(x, y) is null)
                        continue;

                    if (x != 0 && y != 0 && (FindNeighbour(x, 0) is null || FindNeighbour(0, y) is null))
                        continue;

                    mask += 1 << index - 1;
                }
            }

            _sprite.Frame = Content.GetTileFrame(mask);
        }

        public AutoTile GetNeighbour(int x, int y)
        {
            if (_neighbours.TryGetValue(new Vector2(x, y), out AutoTile neighbour))
                return neighbour;

            return null;
        }

        private AutoTile FindNeighbour(int x, int y)
        {
            AutoTile neighbour = GetNeighbour(x, y);

            if (neighbour is not null)
                return neighbour;

            var offset = new Vector2(x, y);
            neighbour = Scene.CheckPosition<AutoTile>(Position + offset * Size);

            if (neighbour is not null && neighbour.Type == Type)
            {
                _neighbours[offset] = neighbour;
                
                return neighbour;
            }

            return default;
        }
    }
}
