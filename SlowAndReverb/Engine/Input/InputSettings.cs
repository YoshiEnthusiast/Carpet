using System.Collections.Generic;
using System.Xml;

namespace SlowAndReverb
{
    public class InputSettings
    {
        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Axis> _axes = new Dictionary<string, Axis>();
        private readonly Dictionary<string, Key> _keys = new Dictionary<string, Key>();

        public InputSettings(XmlElement settings)
        {
            foreach (XmlElement setting in settings["Keyboard"])
            {
                string settingName = setting.GetAttribute("Name");
                Key key = setting.GetEnum<Key>("Value");

                _keys[settingName] = key;
            }

            XmlElement controllerSettings = settings["Controller"];

            foreach (XmlElement setting in controllerSettings.GetElementsByTagName("Button"))
            {
                string settingName = setting.GetAttribute("Name");
                Button button = setting.GetEnum<Button>("Value");

                _buttons[settingName] = button;
            }

            foreach (XmlElement setting in controllerSettings.GetElementsByTagName("Axis"))
            {
                string settingName = setting.GetAttribute("Name");
                Axis axis = setting.GetEnum<Axis>("Value");

                _axes[settingName] = axis;
            }
        }

        public Key GetKey(string name)
        {
            return _keys.GetValueOrDefault(name);
        }

        public Button GetButton(string name)
        {
            return _buttons.GetValueOrDefault(name);
        }

        public Axis GetAxis(string name)
        {
            return _axes.GetValueOrDefault(name);
        } 

        public void SetKey(string name, Key key)
        {
            _keys[name] = key;
        }

        public void SetButton(string name, Button button)
        {
            _buttons[name] = button;
        }

        public void SetAxis(string name, Axis axis)
        {
            _axes[name] = axis;
        }

        public void Save(XmlElement into)
        {
            // I will make this later...
        }
    }
}
