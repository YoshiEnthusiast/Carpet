using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class UIMenu : UIElement
    {
        //OnOpen, OnClose...

        protected T Add<T>(T element) where T : UIElement
        {
            Children.Add(element);

            return element;
        }
    }
}
