using System;
using System.Collections;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public class StackPanel : UIElement
    {
        private readonly List<UIElement> _items = new List<UIElement>();

        public HorizontalAlignment ItemAlignment { get; set; } = HorizontalAlignment.Center;
        public float Margin { get; set; } = 5f;

        protected override void Update(float deltaTime)
        {
            //float totalHeight = Margin * (_items.Count - 1);

            //foreach (UIElement item in _items) 
            //    totalHeight += item.Height;

            //float currentY = HalfHeight - totalHeight / 2f;
            float currentY = 0f;

            foreach (UIElement item in _items)
            {
                float x = ItemAlignment switch
                {
                    HorizontalAlignment.Left => 0f,

                    HorizontalAlignment.Center => HalfWidth - item.HalfWidth,

                    HorizontalAlignment.Right => Rectangle.Right - item.Width
                };

                item.Position = new Vector2(x, currentY);

                currentY += item.Height + Margin;
            }
        }

        public void Add(UIElement item)
        {
            if (_items.Contains(item))
                return;

            _items.Add(item);
            Children.Add(item);
        }

        public void Insert(UIElement item, int index)
        {
            if (_items.Contains(item))
                return;

            int count = _items.Count;

            if (count < 1)
                index = 0;
            else
                index = Maths.Min(index, count - 1);

            _items.Insert(index, item);
            Children.Add(item);
        }

        public void Remove(UIElement item)
        {
            _items.Remove(item);
            Children.Remove(item);
        }

        public void RemoveAt(int index)
        {
            int count = _items.Count;

            if (count < 1)
                return;

            index = Maths.Max(index, count - 1);

            _items.RemoveAt(index);

            UIElement item = _items[index];
            Children.Remove(item);
        }
    }
}
