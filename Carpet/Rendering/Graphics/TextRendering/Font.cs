using System;

namespace Carpet
{
    public class Font
    {
        private const char NewLine = '\n';
        
        private readonly FontFamily _family;

        public Font(string fileName)
        {
            _family = Content.GetFontFamily(fileName);
        }

        public string FamilyName => _family.Name;
        public float LineHeight => _family.LineHeight * Scale.Y;

        public TextMaterial Material { get; set; } = new();
        public Vector2 Scale { get; set; } = Vector2.One;
        public Color OutlineColor { get; set; }
        public int OutlineWidth { get; set; }
        public float BottomMargin { get; set; }
        public float AdditionalAdvance { get; set; }
        public float MaxWidth { get; set; }
        public int MaxRows { get; set; }
        public bool Multiline { get; set; } = true;

        public float NewLineOffset => LineHeight + BottomMargin;

        public void Draw(ReadOnlySpan<char> text, Vector2 position, Color color, float depth, out Vector2 size)
        {
            if (text.Length < 1)
            {
                size = Vector2.Zero;

                return;
            }

            Material.OutlineColor = OutlineColor;
            Material.OutlineWidth = Maths.Min(OutlineWidth, _family.TexturePadding);

            float width = 0f;

            float xOffset = 0f;
            int rows = 1;

            foreach (char symbol in text)
            {
                if (!_family.TryGetCharacter(symbol, out Character character))
                    continue;

                VirtualTexture texture = _family.Texture;

                Rectangle bounds = character.TextureBounds;
                Vector2 bearing = character.Bearing;

                float advance = GetCharacterAdvance(character);

                if (MaxWidth > 0 && xOffset + advance > MaxWidth || symbol == NewLine)
                {
                    if (!Multiline)
                        goto exit;

                    width = Math.Max(width, xOffset);

                    rows++;
                    xOffset = 0f;
                }

                float characterX = xOffset + bearing.X * Scale.X;
                float characterY = (rows - 1) * NewLineOffset + bearing.Y * Scale.Y;

                Graphics.Draw(texture, Material, bounds, position + new Vector2(characterX, characterY), Scale, Vector2.Zero, color, 0f, SpriteEffect.None, SpriteEffect.None, depth);

                xOffset += advance;
            }

exit:
            width = Math.Max(width, xOffset);
            size = new Vector2(width, rows * NewLineOffset - BottomMargin);
        }

        public void Draw(ReadOnlySpan<char> text, float x, float y, Color color, float depth, out Vector2 size)
        {
            Draw(text, new Vector2(x, y), color, depth, out size);
        }

        public void Draw(ReadOnlySpan<char> text, Vector2 position, Color color, float depth)
        {
            Draw(text, position, color, depth, out _);
        }

        public void Draw(ReadOnlySpan<char> text, float x, float y, Color color, float depth)
        {
            Draw(text, x, y, color, depth, out _);
        }

        public Vector2 Measure(ReadOnlySpan<char> text)
        {
            if (text.Length < 1)
                return Vector2.Zero;

            float width = 0f;
            float height = LineHeight;

            float currentWidth = 0f;

            foreach (char symbol in text)
            {
                if (!_family.TryGetCharacter(symbol, out Character character))
                    continue;

                float advance = GetCharacterAdvance(character);

                if (MaxWidth > 0 && currentWidth + advance > MaxWidth || symbol == NewLine)
                {
                    if (!Multiline)
                        return new Vector2(currentWidth, height);

                    width = Maths.Max(width, currentWidth);
                    currentWidth = 0f;

                    height += NewLineOffset;
                }

                currentWidth += advance;
            }

            width = Math.Max(width, currentWidth);
            return new Vector2(width, height);
        }

        public float MeasureX(ReadOnlySpan<char> text)
        {
            return Measure(text).X;
        }

        public float MeasureY(ReadOnlySpan<char> text)
        {
            return Measure(text).Y;
        }

        private float GetCharacterAdvance(Character character)
        {
            return character.Advance * Scale.X + AdditionalAdvance;
        }
    }
}
