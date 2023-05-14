using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class UIListLeftRight<T> : UILeftRight
    {
        private readonly T[] _items;
        private int _selectedIndex = -1;

        public UIListLeftRight(IEnumerable<T> items)
        {
            _items = items.ToArray();

            SelectedIndex = 0;
        }

        public T SelectedItem { get; private set; }
        public Func<T, string> GetItemName { get; set; } = (T item) => item.ToString();

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_items.Length < 1)
                    return;

                value = Maths.Clamp(value, 0, _items.Length - 1);

                if (value != _selectedIndex)
                {
                    _selectedIndex = value;
                    SelectedItem = _items[value];

                    DisplayedItem = GetItemName(SelectedItem);
                }
            }
        }

        protected override void OnLeftPressed()
        {
            SelectedIndex--;
        }

        protected override void OnRightPressed()
        {
            SelectedIndex++;
        }
    }
}
