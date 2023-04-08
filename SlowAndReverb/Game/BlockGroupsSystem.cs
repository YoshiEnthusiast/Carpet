﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class BlockGroupsSystem : System
    {
        public BlockGroupsSystem(Scene scene) : base(scene)
        {

        }

        public override void Initialize()
        {
            var groups = new List<Group>();
            var groupedBlocks = new HashSet<Block>();

            // what do i do
            foreach (Block block in Scene.World.GetEntitiesOfType<Block>())
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

                Block leftNeighbour = GetLeftNeightbour(block);
                Block rightNeighbour = GetRightNeighbour(block);

                while (leftNeighbour is not null)
                {
                    group.Width += leftNeighbour.Width;
                    group.Position = leftNeighbour.TopLeft;

                    group.AddBlock(leftNeighbour);
                    groupedBlocks.Add(leftNeighbour);

                    leftNeighbour = GetLeftNeightbour(leftNeighbour);
                }

                while (rightNeighbour is not null)
                {
                    group.Width += rightNeighbour.Width;

                    group.AddBlock(rightNeighbour);
                    groupedBlocks.Add(rightNeighbour);

                    rightNeighbour = GetRightNeighbour(rightNeighbour);
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

                IEnumerable<Block> blocks = group.Blocks;

                var blockGroup = new BlockGroup(x, y, blocks)
                {
                    Size = new Vector2(width, height)
                };

                foreach (Block block in blocks)
                    block.Group = blockGroup;

                Scene.Add(blockGroup);
            }
        }

        private Block GetLeftNeightbour(Block block)
        {
            return block.GetNeighbour(-1, 0);
        }

        private Block GetRightNeighbour(Block block)
        {
            return block.GetNeighbour(1, 0);
        }

        private sealed class Group
        {
            private readonly List<Block> _blocks = new List<Block>();

            public Vector2 Position { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }

            public IEnumerable<Block> Blocks => _blocks;

            public void AddBlock(Block block)
            {
                _blocks.Add(block);
            }

            public void AddBlocks(IEnumerable<Block> blocks)
            {
                _blocks.AddRange(blocks);
            }
        }
    }
}
