using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Text;
using OpenTKKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using OpenTKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace SlowAndReverb
{
    public static class Input
    {
        private static readonly List<Key> s_pressedKeys = new List<Key>();
        private static readonly List<Key> s_repeatedPressedKeys = new List<Key>();

        private static readonly List<TextInputKey> s_textInput = new List<TextInputKey>();

        private static GameWindow s_window;

        public static Vector2 MousePosition
        {
            get
            {
                if (s_window is null)
                    return Vector2.Zero;

                return s_window.MousePosition;
            }
        }

        public static IEnumerable<Key> PressedKeys => s_pressedKeys;    
        public static IEnumerable<Key> RepeatedPressedKeys => s_repeatedPressedKeys;

        internal static void Initialize(GameWindow window)
        {
            s_window = window;

            s_window.KeyDown += OnKeyDown;
            s_window.TextInput += OnTextInput;
        }

        public static void Update()
        {
            s_pressedKeys.Clear();
            s_repeatedPressedKeys.Clear();

            s_textInput.Clear();
        }

        public static string UpdateTextInputString(string line, int position)
        {
            var builder = new StringBuilder(line);

            foreach (TextInputKey textKey in s_textInput)
            {
                SpecialKey? specialKey = textKey.SpecialKey;

                if (specialKey is not null)
                {
                    SpecialKey specialKeyValue = specialKey.Value;

                    if (specialKeyValue == SpecialKey.Enter)
                    {
                        builder.Insert(position, '\n');

                        position++;
                    }
                    else
                    {
                        if (specialKeyValue == SpecialKey.Backspace)
                        {
                            if (position < 1)
                                continue;

                            builder.Remove(position - 1, 1);

                            position--;
                        }
                        else
                        {
                            if (position > line.Length - 1)
                                continue;

                            builder.Remove(position, 1);
                        }
                    }
                }
                else
                {
                    char? character = textKey.Character;

                    if (character is not null)
                    {
                        builder.Insert(position, character.Value);

                        position++;
                    }
                }
            }

            return builder.ToString();
        }

        public static string UpdateTextInputString(string line)
        {
            return UpdateTextInputString(line, line.Length);
        }

        public static bool IsPressed(Key key)
        {
            return s_pressedKeys.Contains(key);
        }

        public static bool IsRepeatedPressed(Key key)
        {
            return s_repeatedPressedKeys.Contains(key);
        }

        public static bool IsDown(Key key)
        {
            return s_window.IsKeyDown(ToOpenTKKey(key));
        }

        public static bool IsReleased(Key key)
        {
            return s_window.IsKeyReleased(ToOpenTKKey(key));
        }

        public static bool IsMousePressed(MouseButton button)
        {
            return s_window.IsMouseButtonPressed(ToOpenTKMouseButton(button));
        }

        public static bool IsMouseDown(MouseButton button)
        {
            return s_window.IsMouseButtonDown(ToOpenTKMouseButton(button));
        }

        public static bool IsMouseReleased(MouseButton button)
        {
            return s_window.IsMouseButtonReleased(ToOpenTKMouseButton(button));
        }

        private static void OnTextInput(TextInputEventArgs args)
        {
            string line = char.ConvertFromUtf32(args.Unicode);

            if (string.IsNullOrEmpty(line))
                return;

            s_textInput.Add(new TextInputKey()
            {
                Character = line[0]
            });
        }

        private static void OnKeyDown(KeyboardKeyEventArgs args)
        {
            Key key = (Key)args.Key;

            if (key == Key.Enter || key == Key.Backspace || key == Key.Delete)
            {
                SpecialKey specialKey = (SpecialKey)key;

                s_textInput.Add(new TextInputKey()
                {
                    SpecialKey = specialKey
                });
            }

            if (!args.IsRepeat)
                s_pressedKeys.Add(key);

            s_repeatedPressedKeys.Add(key);
        }

        private static OpenTKKey ToOpenTKKey(Key key)
        {
            return (OpenTKKey)key;
        }

        private static OpenTKMouseButton ToOpenTKMouseButton(MouseButton button)
        {
            return (OpenTKMouseButton)button;
        }
    }
}
