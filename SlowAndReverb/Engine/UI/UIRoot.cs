using System.Collections.Generic;

namespace SlowAndReverb
{
    public class UIRoot
    {
        private readonly Stack<UIMenu> _previousMenus = new Stack<UIMenu>();
        private UIMenu _currentMenu;

        public virtual void Update(float deltaTime)
        {
            _currentMenu?.DoUpdate(deltaTime);  
        }

        public virtual void Draw()
        {
            _currentMenu?.DoDraw();
        }

        public void Open(UIMenu menu)
        {
            if (_currentMenu is not null)
                _previousMenus.Push(_currentMenu);

            _currentMenu = menu;
        }

        public void GoBack()
        {
            if (_previousMenus.Count > 0)
                _currentMenu = _previousMenus.Pop();
        }

        public void Close()
        {
            _currentMenu = null;

            _previousMenus.Clear();
        }
    }
}
