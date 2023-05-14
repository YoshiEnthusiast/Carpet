using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class UIElementCollection : DynamicCollection<UIElement>
    {
        private readonly UIElement _parent;

        public UIElementCollection(UIElement parent)
        {
            _parent = parent;
        }

        protected override void OnItemAdded(UIElement item)
        {
            item.Added(_parent);
        }

        protected override void OnItemRemoved(UIElement item)
        {
            item.Removed();
        }
    }
}
