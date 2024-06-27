using System;
using System.Collections.Generic;
using System.Xml;

namespace Carpet.Platforming
{
    public class InputSettings
    {
        private readonly Dictionary<string, Button> _buttons = [];
        private readonly Dictionary<string, Axis> _axes = [];
        private readonly Dictionary<string, Key> _keys = [];

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

        public XmlDocument Save()
        {
            var document = new XmlDocument();

            XmlElement settings = document.CreateElement("Settings");
            document.AppendChild(settings);

            XmlElement keyboard = document.CreateElement("Keyboard");

            foreach (string name in _keys.Keys)
            {
                XmlElement keyboardSetting = document.CreateElement("Key");

                keyboardSetting.SetAttribute("Name", name);
                keyboardSetting.SetAttribute("Value", _keys[name]); 

                keyboard.AppendChild(keyboardSetting);
            }

            settings.AppendChild(keyboard);

            XmlElement controller = document.CreateElement("Controller");

            foreach (string name in _buttons.Keys)
            {
                XmlElement buttonSetting = document.CreateElement("Button");

                buttonSetting.SetAttribute("Name", name);
                buttonSetting.SetAttribute("Value", _buttons[name]);

                controller.AppendChild(buttonSetting);
            }

            foreach (string name in _axes.Keys)
            {
                XmlElement axisSetting = document.CreateElement("Axis");

                axisSetting.SetAttribute("Name", name);
                axisSetting.SetAttribute("Value", _axes[name]);

                controller.AppendChild(axisSetting);
            }

            settings.AppendChild(controller);

            return document;
        }
    }
}
