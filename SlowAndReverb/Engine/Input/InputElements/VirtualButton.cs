using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Carpet
{
    public class VirtualButton : InputElement
    {
        private readonly HashSet<Button> _buttons = [];
        private readonly HashSet<Key> _keys = [];

        private bool _isRepeated;
        private float _repeatTimer;

        public VirtualButton(IEnumerable<Button> buttons, IEnumerable<Key> keys)
        {
            _buttons = buttons.ToHashSet();
            _keys = keys.ToHashSet();
        }

        public VirtualButton()
        {

        }

        public float RepeatInterval { get; set; } = 4f;
        public float RepeatDelay { get; set; } = 25f;

        public override void Update(float deltaTime)
        {
            _isRepeated = false;
           
            if (IsPressed())
            {
                _isRepeated = true;

                _repeatTimer = RepeatDelay;
            }
            else
            {
                if (IsDown())
                {
                    _repeatTimer -= deltaTime;

                    if (_repeatTimer <= 0f)
                    {
                        _isRepeated = true;

                        _repeatTimer = RepeatInterval;
                    } 
                }
            }
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

        public bool IsRepeated()
        {
            return _isRepeated;
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
