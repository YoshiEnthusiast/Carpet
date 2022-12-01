using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class Font
    {
        private readonly FontFamily _family;
        private readonly char _newLine = '\n';

        public Font(string fileName)
        {
            _family = Content.GetFontFamily(fileName);
        }

        public string FamilyName => _family.Name;
        public float LineHeight => _family.LineHeight * Scale.Y;

        public TextMaterial Material { get; set; } = new TextMaterial();
        public Vector2 Scale { get; set; } = Vector2.One;
        public Color Color { get; set; } = Color.White;
        public Color OutlineColor { get; set; }
        public int OutlineWidth { get; set; }
        public float BottomMargin { get; set; }
        public float AdditionalAdvance { get; set; }
        public float MaxWidth { get; set; }
        public bool Multiline { get; set; }

        private float NewLineOffset => LineHeight + BottomMargin;

        public void Draw(string text, Vector2 position, float depth)
        {
            if (string.IsNullOrEmpty(text))
                return;

            Material.Color = Color;
            Material.OutlineColor = OutlineColor;
            Material.OutlineWidth = Math.Min(OutlineWidth, _family.TexturePadding);

            float xOffset = 0f;
            float yOffset = 0f;

            foreach (char symbol in text.ToCharArray())
            {
                if (!_family.TryGetCharacter(symbol, out Character character))
                    continue;

                Texture texture = _family.Texture;

                Rectangle bounds = character.TextureBounds;
                Vector2 bearing = character.Bearing;

                float advance = GetCharacterAdvance(character);

                if (MaxWidth > 0 && xOffset + advance > MaxWidth || symbol == _newLine)
                {
                    if (!Multiline)
                        return;

                    yOffset += NewLineOffset;
                    xOffset = 0f;
                }

                float characterX = xOffset + bearing.X * Scale.X;
                float characterY = yOffset + bearing.Y * Scale.Y;

                Graphics.Draw(texture, Material, bounds, position + new Vector2(characterX, characterY), Scale, Vector2.Zero, 0f, 1f, false, false, depth);

                xOffset += advance;
            }
        }

        public void Draw(string text, float x, float y, float depth)
        {
            Draw(text, new Vector2(x, y), depth);
        }

        public Vector2 Measure(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Vector2.Zero;

            float width = 0f;
            float height = LineHeight;

            float currentWidth = 0f;

            foreach (char symbol in text.ToCharArray())
            {
                if (!_family.TryGetCharacter(symbol, out Character character))
                    continue;

                float advance = GetCharacterAdvance(character);

                if (MaxWidth > 0 && currentWidth + advance > MaxWidth || symbol == _newLine)
                {
                    if (!Multiline)
                        return new Vector2(currentWidth, height);

                    width = Math.Max(width, currentWidth);
                    currentWidth = 0f;

                    height += NewLineOffset;
                }

                currentWidth += advance;
            }

            return new Vector2(Math.Max(width, currentWidth), height);
        }

        private float GetCharacterAdvance(Character character)
        {
            return character.Advance * Scale.X + AdditionalAdvance;
        }
    }
}
