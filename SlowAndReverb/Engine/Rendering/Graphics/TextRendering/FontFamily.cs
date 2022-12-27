using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SlowAndReverb
{
    public sealed class FontFamily
    {
        private readonly Dictionary<char, Character> _characters = new Dictionary<char, Character>();

        private readonly VirtualTexture _texture;
        private readonly string _name;

        private readonly int _padding;
        private readonly int _lineHeight;

        public FontFamily(string textureName, string dataFileName)
        {
            _texture = Content.GetVirtualTexture(textureName);

            XmlDocument document = Utilities.LoadXML(dataFileName);

            XmlElement font = document["font"];
            XmlElement chars = font["chars"];

            foreach (XmlElement symbol in chars)
            {
                if (symbol.Name != "char")
                    continue;

                char key = (char)symbol.GetIntAttribute("id");
                int advance = symbol.GetIntAttribute("xadvance");

                var bounds = new Rectangle(symbol.GetIntAttribute("x"), symbol.GetIntAttribute("y"), symbol.GetIntAttribute("width"), symbol.GetIntAttribute("height"));
                var bearing = new Vector2(symbol.GetIntAttribute("xoffset"), symbol.GetIntAttribute("yoffset"));

                _characters.Add(key, new Character(bounds, bearing, advance));
            }

            XmlElement info = font["info"];
            string[] paddings = info.GetAttribute("padding").Split(',');

            _padding = paddings.Min(padding => Convert.ToInt32(padding));
            _name = info.GetAttribute("face");

            XmlElement common = font["common"];

            _lineHeight = common.GetIntAttribute("lineHeight");
        }

        public VirtualTexture Texture => _texture;
        public string Name => _name;    
        public int TexturePadding => _padding;
        public int LineHeight => _lineHeight;
        
        public bool TryGetCharacter(char symbol, out Character result)
        {
            if (_characters.TryGetValue(symbol, out Character character))
            {
                result = character;

                return true;
            }

            result = default(Character);

            return false;
        }
    }
}
