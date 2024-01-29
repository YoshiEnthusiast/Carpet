using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Carpet
{
    public sealed class FontFamily
    {
        private readonly Dictionary<char, Character> _characters = [];

        public FontFamily(string textureName, string dataFileName)
        {
            Texture = Content.GetVirtualTexture(textureName);

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

            TexturePadding = paddings.Min(padding => Convert.ToInt32(padding));
            Name = info.GetAttribute("face");

            XmlElement common = font["common"];

            LineHeight = common.GetIntAttribute("lineHeight");
        }

        public VirtualTexture Texture { get; private init; }
        public string Name { get; private init; }
        public int TexturePadding { get; private init; }
        public int LineHeight { get; private init; }

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
