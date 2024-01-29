using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml;

namespace Carpet
{
    public sealed class Atlas
    {
        private readonly List<AtlasItem> _items = [];

        private readonly int _maxSize;
        private readonly int _padding;

        public Atlas(int maxSize, int padding)
        {
            _maxSize = maxSize;
            _padding = padding;
        }

        public Texture2D Texture { get; set; }
        public XmlDocument Data { get; set; }

        public bool Build(int preferredSize, int maxSteps)
        {
            if (!CheckSize(preferredSize))
                return false;

            AtlasItem[] orderedItems = _items.OrderByDescending(GetItemArea).ToArray();

            int currentSize = preferredSize;
            int step = preferredSize / 2;
            int stepsTaken = 0;

            PackedTexture[] lastSuccessfulResult = null;

            while (true)
            {
                if (stepsTaken >= maxSteps && lastSuccessfulResult is not null)
                    break;

                if (TryPack(orderedItems, currentSize, out PackedTexture[] result))
                {
                    lastSuccessfulResult = result;

                    stepsTaken++;

                    if (stepsTaken < maxSteps)
                    {
                        currentSize -= step;
                        step /= 2;
                    }

                    continue;
                }

                currentSize += step;

                if (!CheckSize(currentSize))
                    return false;
            }

            SpriteBatch batch = Graphics.SpriteBatch;

            Texture = Texture2D.CreateEmpty(currentSize, currentSize);
            Data = new XmlDocument();

            XmlElement offsets = Data.CreateElement("Offsets");
            Data.AppendChild(offsets); 

            batch.Begin(RenderTarget.FromTexture(Texture), BlendMode.AlphaBlend, Color.Transparent, Matrix4.Identity);

            foreach (PackedTexture packedItem in lastSuccessfulResult)
            {
                AtlasItem item = orderedItems[packedItem.Index];
                Texture2D texture = item.Texture;

                Vector2 position = packedItem.Position;

                XmlElement offset = Data.CreateElement("Offset");

                offset.SetAttribute("Name", item.FileName);
                offset.SetAttribute("X", position.X);
                offset.SetAttribute("Y", position.Y);
                offset.SetAttribute("Width", texture.Width);
                offset.SetAttribute("Height", texture.Height);

                offsets.AppendChild(offset);

                batch.Submit(texture, null, new Vector2(position.X, position.Y), SpriteEffect.None, SpriteEffect.None, 1f);
            }

            batch.End();

            return true;
        }

        public bool Build(int maxSteps)
        {
            int area = GetTotalArea();
            int side = (int)Maths.Ceiling(Maths.Sqrt(area));

            return Build(side, maxSteps);
        }

        public void Add(Texture2D item, string fileName)
        {
            _items.Add(new AtlasItem(item, fileName));
        }

        private bool TryPack(AtlasItem[] orderedItems, int size, out PackedTexture[] result)
        {
            var emptySpaces = new List<Rectangle>()
            {
                new Rectangle(Vector2.Zero, new Vector2(size))
            };

            result = new PackedTexture[orderedItems.Length];

            for (int i = 0; i < orderedItems.Length; i++)
            {
                AtlasItem item = orderedItems[i];
                Texture2D texture = item.Texture;

                int itemWidth = texture.Width + _padding;
                int itemHeight = texture.Height + _padding;

                if (!TryFindEmptySpace(emptySpaces, itemWidth, itemHeight, out int index))
                    return false;

                Rectangle space = emptySpaces[index];

                result[i] = new PackedTexture(i, space.TopLeft);

                float spaceLeft = space.Left;
                float spaceTop = space.Top;
                float spaceWidth = space.Width;
                float spaceHeight = space.Height;

                float deltaWidth = spaceWidth - itemWidth;
                float deltaHeight = spaceHeight - itemHeight;

                float itemRight = spaceLeft + itemWidth;
                float itemBottom = spaceTop + itemHeight;

                emptySpaces.RemoveAt(index);

                if (deltaWidth > deltaHeight)
                {
                    AddSpace(emptySpaces, new Rectangle(spaceLeft, itemBottom, itemWidth, deltaHeight));
                    AddSpace(emptySpaces, new Rectangle(itemRight, spaceTop, deltaWidth, spaceHeight));
                }
                else
                {
                    AddSpace(emptySpaces, new Rectangle(itemRight, spaceTop, deltaWidth, itemHeight));
                    AddSpace(emptySpaces, new Rectangle(spaceLeft, itemBottom, spaceWidth, deltaHeight));
                }
            }

            return true;
        }

        private bool TryFindEmptySpace(List<Rectangle> emptySpaces, int itemWidth, int itemHeight, out int index)
        {
            for (int i = 0; i < emptySpaces.Count; i++)
            {
                Rectangle space = emptySpaces[i];

                if (space.Width >= itemWidth && space.Height > itemHeight)
                {
                    index = i;

                    return true;
                }
            }

            index = default(int);

            return false;
        }

        private void AddSpace(List<Rectangle> to, Rectangle space)
        {
            if (space.Width < 1f || space.Height < 1f)
                return;

            to.Add(space);
        }

        private int GetTotalArea()
        {
            int totalArea = 0;

            foreach (AtlasItem item in _items)
                totalArea += GetItemArea(item);

            return totalArea;
        }

        private int GetItemArea(AtlasItem item)
        {
            Texture2D texture = item.Texture;

            return texture.Width * texture.Height;
        }

        private bool CheckSize(int size)
        {
            return size <= _maxSize;
        }

        private record class AtlasItem(Texture2D Texture, string FileName);

        private readonly record struct PackedTexture(int Index, Vector2 Position);
    }
}
