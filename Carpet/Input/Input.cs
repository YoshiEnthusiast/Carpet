using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using OpenTKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace Carpet
{
    public static class Input
    {
        private const char NewLine = '\n';

        private static readonly List<Key> s_pressedKeys = [];
        private static readonly List<Key> s_repeatedPressedKeys = [];

        private static readonly List<TextInputKey> s_textInput = [];

        private static JoystickState s_activeController;
        private static ControllerMapping s_currentMapping;

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

        public static string ActiveControllerName
        {
            get
            {
                if (s_activeController is null)
                    return null;

                return s_activeController.Name;
            }
        }

        public static float MouseScroll { get; private set; }
        public static InputDeviceType DeviceType { get; private set; }

        public static IEnumerable<Key> PressedKeys => s_pressedKeys;    
        public static IEnumerable<Key> RepeatedPressedKeys => s_repeatedPressedKeys;

        private static IEnumerable<JoystickState> Controllers 
        {
            get
            {
                foreach (JoystickState state in s_window.JoystickStates) 
                    if (state is not null)
                        yield return state;
            }
        }

        internal static void Initialize(GameWindow window)
        {
            s_window = window;

            s_window.KeyDown += OnKeyDown;
            s_window.TextInput += OnTextInput;
            s_window.JoystickConnected += OnJoystickConnected;
            s_window.MouseWheel += OnMouseWheel; 
        }

        public static void Update()
        {
            JoystickState lastActiveController = s_activeController; 
            JoystickState activeController = null;

            bool hasPreviousController = false;

            foreach (JoystickState controller in Controllers)
            {
                for (int i = 0; i < controller.ButtonCount; i++)
                {
                    if (controller.IsButtonPressed(i))
                    {
                        activeController = controller;

                        break;
                    }

                    if (s_activeController is not null && s_activeController.Id == controller.Id)
                        hasPreviousController = true;
                }
            }

            if (activeController is null)
            {
                if (s_activeController is null || !hasPreviousController)
                    s_activeController = Controllers.FirstOrDefault();
            }
            else
            {
                s_activeController = activeController;

                DeviceType = InputDeviceType.Controller;
            }

            if (s_activeController is not null && s_activeController != lastActiveController)
            {
                string name = s_activeController.Name;
                ControllerMapping mapping = Content.GetControllerMapping(name);

                if (mapping is null)
                {
                    Console.WriteLine($"Could not find mapping for \"{name}\". Applying default mapping...");

                    s_currentMapping = Content.GetControllerMapping("default");
                }
                else
                {
                    Console.WriteLine($"Found mapping for \"{name}\"");

                    s_currentMapping = mapping;
                }
            }

            if (s_pressedKeys.Any() || s_activeController is null)
                DeviceType = InputDeviceType.Keyboard;
        }

        public static void Clear()
        {
            s_pressedKeys.Clear();
            s_repeatedPressedKeys.Clear();

            s_textInput.Clear();

            MouseScroll = 0f;
        }

        public static string UpdateTextInputString(string line, ReadOnlySpan<char> ignore, ref int position, out bool textChanged)
        {
            if (s_textInput.Count < 1)
            {
                textChanged = false;

                return line;
            }

            var builder = new StringBuilder(line);

            foreach (TextInputKey textKey in s_textInput)
            {
                SpecialKey? specialKey = textKey.SpecialKey;

                if (specialKey is not null)
                {
                    SpecialKey specialKeyValue = specialKey.Value;

                    if (specialKeyValue == SpecialKey.Enter)
                    {
                        AppendCharacter(builder, NewLine, ignore, ref position);
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
                        char value = character.Value;

                        AppendCharacter(builder, value, ignore, ref position);
                    }
                }
            }

            textChanged = true;

            return builder.ToString();
        }

        #region Input Checks

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

        public static bool IsControllerPressed(int buttonIndex)
        {
            if (s_activeController is null ||
                    buttonIndex >= s_activeController.ButtonCount)
                return false;

            return s_activeController.IsButtonPressed(buttonIndex);
        }

        public static bool IsControllerDown(int buttonIndex)
        {
            if (s_activeController is null ||
                    buttonIndex >= s_activeController.ButtonCount)
                return false;

            return s_activeController.IsButtonDown(buttonIndex);
        }

        public static bool IsControllerReleased(int buttonIndex)
        {
            if (s_activeController is null ||
                    buttonIndex >= s_activeController.ButtonCount)
                return false;

            return s_activeController.IsButtonReleased(buttonIndex);
        }

        public static float GetControllerAxis(int axisIndex)
        {
            if (s_activeController is null ||
                    axisIndex >= s_activeController.AxisCount)
                return 0f;

            return s_activeController.GetAxis(axisIndex);
        }

        public static DpadState GetDpadState(int dpadIndex)
        {
            if (s_activeController is null ||
                    dpadIndex >= s_activeController.HatCount)
                return default;

            return (DpadState)s_activeController.GetHat(dpadIndex);
        }

        public static DpadState GetDpadState()
        {
            return GetDpadState(0);
        }

        public static bool IsControllerPressed(Button button)
        {
            if (s_currentMapping is null)
                return false;

            int id = s_currentMapping.GetButtonId(button);

            return IsControllerPressed(id);
        }

        public static bool IsControllerDown(Button button)
        {
            if (s_currentMapping is null)
                return false;

            int id = s_currentMapping.GetButtonId(button);

            return IsControllerDown(id);
        }

        public static bool IsControllerReleased(Button button)
        {
            if (s_currentMapping is null)
                return false;

            int id = s_currentMapping.GetButtonId(button);

            return IsControllerReleased(id);    
        }

        public static float GetControllerAxis(Axis axis)
        {
            if (s_currentMapping is null)
                return 0f;

            int id = s_currentMapping.GetAxisId(axis);  

            return GetControllerAxis(id);
        }

        #endregion

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

        private static void OnJoystickConnected(JoystickEventArgs args)
        {
            foreach (JoystickState controller in Controllers)
                if (args.JoystickId == controller.Id)
                    Console.WriteLine($"Controller \"{controller.Name}\" connected");
        }

        private static void OnMouseWheel(MouseWheelEventArgs args)
        {
            MouseScroll = args.OffsetY;
        }

        private static OpenTKKey ToOpenTKKey(Key key)
        {
            return (OpenTKKey)key;
        }

        private static OpenTKMouseButton ToOpenTKMouseButton(MouseButton button)
        {
            return (OpenTKMouseButton)button;
        }

        private static void AppendCharacter(StringBuilder builder, char character, ReadOnlySpan<char> ignore, ref int position)
        {
            if (ignore.Contains(character))
                return;

            builder.Insert(position, character);
            position++;
        }
    }
}   
