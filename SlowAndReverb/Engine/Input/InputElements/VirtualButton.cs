using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class VirtualButton : InputElement
    {
        private readonly HashSet<Button> _buttons = new HashSet<Button>();
        private readonly HashSet<Key> _keys = new HashSet<Key>();

        public VirtualButton(IEnumerable<Button> buttons, IEnumerable<Key> keys)
        {
            _buttons = buttons.ToHashSet();
            _keys = keys.ToHashSet();
        }

        public VirtualButton()
        {

        }

        public bool IsPressed()
        {
            foreach (Button button in _buttons)
                if (Input.IsControllerPressed(button))
                    return true;

            foreach (Key key in _keys)
                if (Input.IsPressed(key))
                    return true;

            return false;
        }

        public bool IsDown()
        {
            foreach (Button button in _buttons)
                if (Input.IsControllerDown(button))
                    return true;

            foreach (Key key in _keys)
                if (Input.IsDown(key))
                    return true;

            return false;
        }

        public bool IsReleased()
        {
            foreach (Button button in _buttons)
                if (Input.IsControllerReleased(button))
                    return true;

            foreach (Key key in _keys)
                if (Input.IsReleased(key))
                    return true;

            return false;
        }

        public VirtualButton AddButton(Button button)
        {
            _buttons.Add(button);

            return this;
        }

        public VirtualButton AddKey(Key key)
        {
            _keys.Add(key);

            return this;
        }
    }
}
