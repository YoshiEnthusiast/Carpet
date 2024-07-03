namespace Carpet.Platforming
{
    public class BlockGroupsSystem : System
    {
        private readonly List<AutoTile> _autoTilesBuffer = [];

        public BlockGroupsSystem(Scene scene) : base(scene)
        {

        }

        public override void Initialize()
        {
            var groups = new List<Group>();
            var groupedBlocks = new HashSet<AutoTile>();

            foreach (AutoTile block in Scene.GetEntitiesOfType<AutoTile>(_autoTilesBuffer))
            {
                if (groupedBlocks.Contains(block))
                    continue;

                groupedBlocks.Add(block);

                var group = new Group()
                {
                    Position = block.TopLeft,
                    Height = block.Height,
                    Width = block.Width
                };

                group.AddBlock(block);
                groups.Add(group);

                AutoTile leftNeighbor = GetLeftNeighbor(block);
                AutoTile rightNeighbor = GetRightNeighbor(block);

                while (leftNeighbor is not null)
                {
                    group.Width += leftNeighbor.Width;
                    group.Position = leftNeighbor.TopLeft;

                    group.AddBlock(leftNeighbor);
                    groupedBlocks.Add(leftNeighbor);

                    leftNeighbor = GetLeftNeighbor(leftNeighbor);
                }

                while (rightNeighbor is not null)
                {
                    group.Width += rightNeighbor.Width;

                    group.AddBlock(rightNeighbor);
                    groupedBlocks.Add(rightNeighbor);

                    rightNeighbor = GetRightNeighbor(rightNeighbor);
                }
            }

            IEnumerable<Group> orderedGroups = groups.OrderByDescending(group => group.Position.Y);

            foreach (Group group in orderedGroups)
            {
                float height = group.Height;

                if (height <= 0f)
                    continue;

                foreach (Group another in orderedGroups)
                {
                    float anotherHeight = another.Height;

                    if (anotherHeight <= 0f)
                        continue;

                    Vector2 position = group.Position;
                    Vector2 anotherPosition = another.Position;

                    if (group.Width == another.Width && position.X == anotherPosition.X && position.Y + height == anotherPosition.Y)
                    {
                        group.AddBlocks(another.Blocks);
                        group.Height += anotherHeight;

                        another.Height = 0f;
                    }
                }
            }

            foreach (Group group in groups)
            {
                float width = group.Width;
                float height = group.Height;

                if (height <= 0f)
                    continue;

                Vector2 position = group.Position;

                float x = position.X + width / 2f;
                float y = position.Y + height / 2f;

                IEnumerable<AutoTile> blocks = group.Blocks;

                var blockGroup = new BlockGroup(x, y, blocks)
                {
                    Size = new Vector2(width, height)
                };

                foreach (AutoTile block in blocks)
                    block.Group = blockGroup;

                Scene.Add(blockGroup);
            }
        }

        private AutoTile GetLeftNeighbor(AutoTile block)
        {
            return block.GetNeighbor(-1, 0);
        }

        private AutoTile GetRightNeighbor(AutoTile block)
        {
            return block.GetNeighbor(1, 0);
        }

        private sealed class Group
        {
            private readonly List<AutoTile> _blocks = new List<AutoTile>();

            public Vector2 Position { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }

            public IEnumerable<AutoTile> Blocks => _blocks;

            public void AddBlock(AutoTile block)
            {
                _blocks.Add(block);
            }

            public void AddBlocks(IEnumerable<AutoTile> blocks)
            {
                _blocks.AddRange(blocks);
            }
        }
    }
}
