using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class StackMenu : InGameMenu
    {
        private readonly StackPanel _itemsPanel;
        private readonly List<UIMenuItem> _items = new List<UIMenuItem>();

        private int _selectedIndex;

        public StackMenu()
        {
            _itemsPanel = Add(new StackPanel());
        }

        public float Padding { get; set; } = 10f;

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            var padding = new Vector2(Padding);
            _itemsPanel.Size = Size - padding * 2f;
            _itemsPanel.Position = padding + new Vector2(0f, HeaderHeight);

            int itemsCount = _items.Count;

            if (itemsCount > 1)
            {
                UIMenuItem selectedItem = _items[_selectedIndex];

                // will be replaced...
                if (Input.IsPressed(Key.S))
                {
                    if (_selectedIndex < itemsCount - 1)
                    {
                        selectedItem.Selected = false;
                        _selectedIndex++;

                        _items[_selectedIndex].Selected = true;
                    }
                }
                else if (Input.IsPressed(Key.W))
                {
                    if (_selectedIndex > 0)
                    {
                        selectedItem.Selected = false;
                        _selectedIndex--;

                        _items[_selectedIndex].Selected = true;
                    }
                }
            }
        }

        protected T AddItem<T>(T item) where T : UIMenuItem
        {
            if (_items.Count < 1)
                item.Selected = true;

            _items.Add(item);
            _itemsPanel.Add(item);

            return item;
        }
    }
}
