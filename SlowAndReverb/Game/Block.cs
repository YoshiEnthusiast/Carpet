using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlowAndReverb
{
    public class Block : SolidObject
    {
        private readonly Sprite _sprite;
        private readonly Dictionary<Vector2, Block> _neighbours = new Dictionary<Vector2, Block>();

        private bool _fake;

        public Block(string tileSet, float x, float y) : base(x, y)
        {
            Size = new Vector2(8f);
            TileSet = tileSet;

            _sprite = Add(new Sprite(tileSet, (int)Width, (int)Height));
        }

        public bool Fake
        {
            get
            {
                return _fake;
            }

            set
            {
                if (_fake == value)
                    return;

                NeedsRefresh = true;
                IgnoreCollisions = value;

                _fake = value;
            }
        }

        public BlockGroup Group { get; set; }

        public string TileSet { get; private init; }
        public bool NeedsRefresh { get; set; } = true;

        protected override void Update(float deltaTime)
        {
            if (NeedsRefresh)
            {
                Refresh();

                NeedsRefresh = false;
            }
            
            if (Fake && Scene.CheckRectangle<Player>(Rectangle) is not null)
            {
                Awake = false;

                Group?.FadeAway();
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
                    if ((x == 0 && y == 0))
                        continue;

                    index++;

                    if (FindNeighbour(x, y) is null)
                        continue;

                    if (x != 0 && y != 0 && (FindNeighbour(x, 0) is null || FindNeighbour(0, y) is null))
                        continue;

                    mask += 1 << (index - 1);
                }
            }

            _sprite.Frame = Content.GetTileFrame(mask);
        }

        public Block GetNeighbour(int x, int y)
        {
            if (_neighbours.TryGetValue(new Vector2(x, y), out Block neighbour)) 
                return neighbour;

            return null;
        }

        private Block FindNeighbour(int x, int y)
        {
            Block neighbour = GetNeighbour(x, y);

            if (neighbour is not null)
                return neighbour;

            var offset = new Vector2(x, y);
            neighbour = Scene.CheckPoint<Block>(Position + offset * Size);

            if (neighbour is not null && neighbour.TileSet == TileSet && neighbour.Fake == Fake)
                _neighbours[offset] = neighbour;

            return neighbour;
        }
    }
}
