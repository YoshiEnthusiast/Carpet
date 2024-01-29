using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Carpet
{
    public sealed class ControllerMapping
    {
        private readonly Dictionary<Button, int> _buttons = [];
        private readonly Dictionary<Axis, int> _axes = [];

        public ControllerMapping(XmlElement mapping)
        {
            foreach (XmlElement buttonMapping in mapping["Buttons"])
            {
                Button button = buttonMapping.GetEnum<Button>("Name");
                int id = buttonMapping.GetIntAttribute("Id");

                _buttons[button] = id;
            }

            foreach (XmlElement axisMapping in mapping["Axes"])
            {
                Axis axis = axisMapping.GetEnum<Axis>("Name");
                int id = axisMapping.GetIntAttribute("Id");

                _axes[axis] = id;
            }
        }

        public int GetButtonId(Button button)
        {
            return _buttons[button];
        }

        public int GetAxisId(Axis axis)
        {
            return _axes[axis];
        }
    }
}
