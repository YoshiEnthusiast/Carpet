using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace Carpet
{
    // TODO: Test with different fonts

    public static class DebugConsole
    {
        private const string UnexpectedQuotesError = "Unexpected quotes";
        private const string UnclosedQuotesError = "Unclosed quotes";
        private const string NullStringMessage = "[null string]";
        private const string EmptyStringMessage = "[empty string]";
        private const string InputSign = "< ";
        private const string OutputSign = "> ";

        private const char Space = ' ';
        private const char Quotes = '"';
        private const char VerticalLine = '|';
        private const char SemiColon = ';';
        private const char BackSlash = '\\';

        private const float PositionLerpAmount = 0.2f;
        private const float CarriageSinMultiplier = 5f;
        private const float LineWidth = 1f;

        private const int MaxElements = 100; 
        private const int MaxCommands = 20;
        private const int MaxLogs = 200;

        private const int DefaultPossibleCommandIndex = -1;
        private const int DefaultCommandHistoryIndex = -1;

        private static readonly Dictionary<string, Command> s_commands = [];

        private static readonly PreprocessedCommand[] s_preprocessedCommands = new PreprocessedCommand[MaxCommands];
        private static readonly PreprocessedCommand[] s_temporaryCommandBuffer = new PreprocessedCommand[MaxCommands];

        private static readonly ConsoleLog?[] s_logs = new ConsoleLog?[MaxLogs];
        private static readonly List<string> s_possibleCommands = [];
        private static readonly List<string> s_commandHistory = [];

        private static readonly Stack<Element[]> s_elementBufferCache = [];
        private static readonly StringBuilder s_builder = new();

        private static readonly Dictionary<Key, string> s_binds = [];
        private static readonly Dictionary<string, string> s_aliases = [];

        private static readonly Dictionary<LogType, LogPrefix> s_logPrefixes = new()
        {
            [LogType.Info] = new LogPrefix("INFO", Color.Blue),
            [LogType.Warning] = new LogPrefix("WARN", Color.Yellow),
            [LogType.Error] = new LogPrefix("ERR", Color.Red),
            [LogType.OK] = new LogPrefix("OK", Color.LightGreen),
            [LogType.OpenGL] = new LogPrefix("GL", Color.Purple)
        };

        private static readonly char[] s_ignore =
        {
            '\n',
            '`'
        };
        
        private static Layer s_layer;

        private static Font s_logsFont;
        private static Font s_inputFont;

        private static float s_y;

        private static float s_logsHeight;
        private static float s_scrollValue;

        private static string s_inputString = string.Empty;
        private static string s_autoCompleteString = string.Empty;
        private static string s_autoCompleteElement = string.Empty;

        private static int s_commandsCount;

        private static Vector2 s_size = new(250f, 140f);
        private static float s_textPadding = 1f;
        private static float s_scrollBarWidth = 2f;

        private static int s_possibleCommandIndex = DefaultPossibleCommandIndex; 
        private static int s_commandHistoryIndex = 0;

        private static float s_inputViewOffset;
        private static int s_carriageIndex;

        private static bool s_initialized;

        public static Vector2 Size
        {
            get
            {
                return s_size;
            }


            set
            {
                s_size = value;

                RecalculateLogsHeight();
            }
        } 

        public static float TextPadding
        {
            get
            {
                return s_textPadding;
            }

            set
            {
                s_textPadding = value;

                RecalculateLogsHeight();
            }
        } 

        public static float ScrollBarWidth
        {
            get
            {
                return s_scrollBarWidth;
            }

            set
            {
                s_scrollBarWidth = value;

                RecalculateLogsHeight();
            }
        }

        public static Color MainColor { get; set; } = Color.White;
        public static Color BackgroundColor { get; set; } = Color.Black;
        public static float Depth { get; set; } = 0f;

        public static Key ToggleKey { get; set; } = Key.GraveAccent;
        public static float MouseScrollMultiplier { get; set; } = 0.2f;
        public static bool CatchRuntimeErrors { get; set; } = false;

        public static bool Open { get; private set; }

        public static void Initialize(Layer layer, string fontFamily)
        {
            if (s_initialized)
                throw new InvalidOperationException("DebugConsole has already " +
                    "been initialized");

            s_initialized = true;

            Argument.Initialize();

            s_layer = layer;
            s_y = -Size.Y;

            s_logsFont = new Font(fontFamily);

            s_inputFont = new Font(fontFamily)
            {
                Multiline = false
            };

            SetLogsFontMaxWidth();

            AddCommand("log", new Argument[]
            {
                new Argument.String("text"),
                new Argument.Int("r", true),
                new Argument.Int("g", true),
                new Argument.Int("b", true)
            }, 
            (Arguments arguments) =>
            {
                string text = arguments.Get<string>("text");

                if (!arguments.Given("r"))
                {
                    Log(text, MainColor);

                    return;
                }

                var color = new Color(arguments.Get<int>("r"),
                    arguments.Get<int>("g"), arguments.Get<int>("b"));

                Log(text, color);
            });

            AddCommand("logt", new Argument[]
            {
                new Argument.String("text"),
                new Argument.Enum<LogType>("type")
            },
            (Arguments arguments) =>
            {
                string text = arguments.Get<string>("text");
                LogType type = arguments.Get<LogType>("type");

                Log(text, type);
            });

            AddCommand("bind", new Argument[]
            {
                new Argument.Enum<Key>("key"),
                new Argument.String("command", true)
            },
            (Arguments arguments) =>
            {
                Key key = arguments.Get<Key>("key");

                if (arguments.TryGet("command", out string newCommand))
                {
                    s_binds[key] = newCommand;
                }
                else
                {
                    if (s_binds.TryGetValue(key, out string command))
                        Log($"\"{command}\"");
                    else
                        Log($"Nothing bound to key {key}");
                }
            });

            AddCommand("do", new Argument[]
            {
                new Argument.String("command"),
                new Argument.Int("count")
            },
            (Arguments arguments) =>
            {
                string command = arguments.Get<string>("command");
                int count = arguments.Get<int>("count");

                for (int i = 0; i < count; i++)
                    Run(command);
            });

            AddCommand("alias", new Argument[]
            {
                new Argument.String("name"),
                new Argument.String("value", true)
            },
            (Arguments arguments) =>
            {
                string name = arguments.Get<string>("name");

                if (name.Contains(Space))
                {
                    Log("Aliases cannot contain white spaces");

                    return;
                }

                if (arguments.TryGet("value", out string newValue))
                {
                    s_aliases[name] = newValue;
                }
                else
                {
                    if (s_aliases.TryGetValue(name, out string value))
                        Log(value);
                    else
                        Log($"alias \"{name}\" does not exist");
                }
            });

            AddCommand("clear", null, (Arguments arguments) =>
            {
                Clear();
            });

            AddCommand("size", new Argument[]
            {
                new Argument.Int("width"),
                new Argument.Int("height")
            },
            (Arguments arguments) =>
            {
                float width = arguments.Get<int>("width");
                float height = arguments.Get<int>("height");

                Size = new Vector2(width, height);
            });

            AddCommand("test1", new Argument[]
            {

            }, (Arguments arguments) =>
            {

            });
            AddCommand("test2", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test3", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test4", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test5", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test6", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test7", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test8", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test9", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test10", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test11", null, (Arguments arguments) =>
            {
                
            });
            AddCommand("test12", null, (Arguments arguments) =>
            {
                
            });
        }

        public static void Update(float deltaTime)
        {
            float height = Size.Y;

            if (Input.IsPressed(ToggleKey))
                Open = !Open;

            if (!Open || Input.IsDown(Key.LeftControl))
            {
                foreach (Key key in s_binds.Keys)
                {
                    if (Input.IsPressed(key))
                    {
                        string command = s_binds[key];

                        Run(command);
                    }
                }
            }
            
            if (Open)
            {
                bool textSwapped = false;
                bool commandNameAutoCompleted = false;

                s_y = Maths.Lerp(s_y, (s_layer.Height - height) / 2f, PositionLerpAmount);

                int commandHistoryLength = s_commandHistory.Count;

                if (commandHistoryLength > 0)
                {
                    if (Input.IsPressed(Key.Up))
                    {
                        if (s_commandHistoryIndex < commandHistoryLength - 1)
                        {
                            s_commandHistoryIndex++;

                            SetInputText(s_commandHistory[s_commandHistoryIndex]);
                            textSwapped = true;
                        }
                    }
                    else if (Input.IsPressed(Key.Down))
                    {
                        if (s_commandHistoryIndex >= 1)
                        {
                            s_commandHistoryIndex--;

                            SetInputText(s_commandHistory[s_commandHistoryIndex]);
                        }
                        else
                        {
                            ClearInput();

                            s_commandHistoryIndex = DefaultCommandHistoryIndex;
                        }

                        textSwapped = true;
                    }
                }

                if (Input.IsPressed(Key.Left))
                {
                    if (s_carriageIndex > 0)
                        s_carriageIndex--;
                }
                else if (Input.IsPressed(Key.Right))
                {
                    if (s_carriageIndex < s_inputString.Length)
                        s_carriageIndex++;
                }

                if (s_carriageIndex == s_inputString.Length && Input.IsPressed(Key.Tab))
                {
                    int possibleCommandsNamesCount = s_possibleCommands.Count;
                    
                    if (possibleCommandsNamesCount > 0)
                    {
                        int previousIndex = s_possibleCommandIndex;

                        if (previousIndex == DefaultPossibleCommandIndex)
                        {
                            s_possibleCommandIndex = 0;
                        }
                        else
                        {
                            int lastIndex = possibleCommandsNamesCount - 1;

                            if (Input.IsDown(Key.LeftShift))
                            {
                                if (s_possibleCommandIndex > 0)
                                    s_possibleCommandIndex--;
                                else
                                    s_possibleCommandIndex = lastIndex;
                            }
                            else
                            {
                                if (s_possibleCommandIndex < lastIndex)
                                    s_possibleCommandIndex++;
                                else
                                    s_possibleCommandIndex = 0;
                            }
                        } 

                        if (s_possibleCommandIndex != previousIndex)
                        {
                            PreprocessedCommand lastCommand = s_preprocessedCommands[s_commandsCount - 1];
                            Element commandElement = lastCommand.Elements[0];
                            string command = s_possibleCommands[s_possibleCommandIndex];

                            s_builder.Clear();

                            int textBeforeCommandLength = s_inputString.Length - 
                                commandElement.InitialString.Length;

                            if (textBeforeCommandLength > 0)
                            {
                                ReadOnlySpan<char> textBeforeCommand = s_inputString.AsSpan(0, textBeforeCommandLength);
                                s_builder.Append(textBeforeCommand);
                            }

                            s_builder.Append(command);

                            s_inputString = s_builder.ToString();
                            s_carriageIndex = s_inputString.Length;

                            commandNameAutoCompleted = true;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(s_autoCompleteElement))
                        {
                            s_inputString += s_autoCompleteElement;
                            s_carriageIndex += s_autoCompleteElement.Length;

                            textSwapped = true;
                        }
                    }
                }

                s_inputString = Input.UpdateTextInputString(s_inputString, s_ignore, ref s_carriageIndex, out bool textEdited);

                if (textSwapped || textEdited || commandNameAutoCompleted)
                {
                    FreeElementBuffers(s_preprocessedCommands, s_commandsCount);
                    Preprocess(s_inputString, s_preprocessedCommands, out s_commandsCount);

                    AutoCompleteData autoComplete = GenerateAutoComplete(s_inputString,
                        s_preprocessedCommands, s_commandsCount);
                    s_autoCompleteString = autoComplete.FullString;
                    s_autoCompleteElement = autoComplete.LastElement;

                    if (textSwapped || textEdited)
                    {
                        FindPossibleCommands(s_possibleCommands, s_inputString,
                            s_preprocessedCommands, s_commandsCount);

                        s_possibleCommandIndex = DefaultPossibleCommandIndex;

                        if (textEdited)
                            s_commandHistoryIndex = DefaultCommandHistoryIndex;
                    }
                }

                if (Input.IsPressed(Key.Enter))
                {
                    if (!string.IsNullOrEmpty(s_inputString))
                    {
                        LogInput(s_inputString, MainColor); 
                        s_scrollValue = 0f;

                        Run(s_preprocessedCommands, s_commandsCount);

                        if (commandHistoryLength < 1 ||
                            s_commandHistory[0] != s_inputString)
                            s_commandHistory.Insert(0, s_inputString);

                        ClearInput();
                    } 
                }

                float scroll = Input.MouseScroll * MouseScrollMultiplier;
                s_scrollValue = Maths.Clamp(s_scrollValue + scroll, 0f, 1f);
            }
            else
            {
                s_y = Maths.Lerp(s_y, -height, PositionLerpAmount);
            }
        }

        public static void Draw()
        {
            float x = (s_layer.Width - Size.X) / 2f;
            float y = Maths.Floor(s_y);
            float width = Size.X;
            float height = Size.Y;

            var position = new Vector2(x, y);
            var rectangle = new Rectangle(position, position + Size); 

            float right = rectangle.Right;
            float bottom = rectangle.Bottom;    

            Graphics.FillRectangle(rectangle, BackgroundColor, Depth);
            Graphics.DrawRectangle(rectangle, MainColor, Depth);

            float inputFontHeight = s_inputFont.LineHeight;
            float inputXOffset = LineWidth + TextPadding * 2f;
            float inputX = x + inputXOffset;
            float inputY = bottom - inputFontHeight - LineWidth;
            float inputRight = right - LineWidth;

            float inputWidth = inputRight - inputX;
            float inputHeight = s_inputFont.LineHeight;

            float barY = inputY - TextPadding - LineWidth;

            Graphics.DrawLine(new Vector2(x, barY), new Vector2(right, barY), 
                MainColor, Depth);

            var inputRectangle = new Rectangle(inputX - TextPadding, inputY, inputWidth + TextPadding, inputHeight);
            Graphics.Scissor = inputRectangle;

            ReadOnlySpan<char> textBeforeCarriage = s_inputString.AsSpan(0, s_carriageIndex);
            float textBeforeCarriageWidth = s_inputFont.MeasureX(textBeforeCarriage);

            float carriageX = textBeforeCarriageWidth - s_inputViewOffset;

            if (carriageX < 0f)
            {
                s_inputViewOffset += carriageX;
                carriageX = 0f;
            }
            else if (carriageX > inputWidth)
            {
                s_inputViewOffset += carriageX - inputWidth;
                carriageX = inputWidth;
            }

            s_inputFont.Draw(s_inputString, inputX - s_inputViewOffset,
                inputY, MainColor, Depth);

            float inputEndX = s_inputFont.MeasureX(s_inputString)
                + x + inputXOffset - s_inputViewOffset;

            Color autoCompleteColor = MainColor * 0.5f;
            s_inputFont.Draw(s_autoCompleteString, inputEndX, inputY, 
                autoCompleteColor, Depth);

            if (Maths.Sin(Engine.TimeElapsedFloat * CarriageSinMultiplier) > 0f)
            {
                float carriageTop = inputY + TextPadding;
                float carriageBottom = inputY + inputFontHeight - TextPadding;

                carriageX += inputX;

                Graphics.DrawLine(new Vector2(carriageX, carriageTop), 
                    new Vector2(carriageX, carriageBottom), MainColor, 0f);
            }

            Graphics.ResetScissor();

            if (Open)
            {
                float possibleCommandY = inputY + inputHeight;
                float possibleCommandX = inputX;

                for (int i = 0; i < s_possibleCommands.Count; i++)
                {
                    string command = s_possibleCommands[i];

                    float textY = possibleCommandY + TextPadding;
                    float textWidth = s_inputFont.MeasureX(command);

                    Color rectangleColor = BackgroundColor;
                    Color textColor = MainColor;

                    if (s_possibleCommandIndex == i)
                    {
                        rectangleColor = MainColor;
                        textColor = BackgroundColor;
                    }

                    var textRectangle = new Rectangle(possibleCommandX, possibleCommandY, textWidth + TextPadding,
                        inputHeight + TextPadding * 2f);

                    Graphics.FillRectangle(textRectangle, rectangleColor, Depth);
                    s_inputFont.Draw(command, possibleCommandX + TextPadding,
                        textY, textColor, Depth);

                    if (possibleCommandX + textWidth >= right)
                    {
                        possibleCommandX = inputX;
                        possibleCommandY += inputHeight + TextPadding * 2f;
                    }
                    else
                    {
                        possibleCommandX += textWidth + TextPadding;
                    }
                }
            }

            float logsOffsetX = CalculateLogsOffsetX();
            float logsTextX = x + logsOffsetX + TextPadding;

            float logsY = y + LineWidth;
            float logsHeight = height - inputFontHeight - TextPadding * 4f - LineWidth * 2f;
            float logsBottom = logsY + logsHeight;

            float logsWindowHeight = logsHeight - TextPadding;

            var logsWindowRectangle = new Rectangle(x, y + TextPadding, width, logsWindowHeight);
            float logsWindowBottom = logsWindowRectangle.Bottom;

            float scrollOffset = (s_logsHeight - logsWindowHeight) * s_scrollValue;
            float currentLogsHeight = -scrollOffset;

            float maxScrollBarHeight = logsWindowHeight - TextPadding * 2f;
            float scrollBarHeight = maxScrollBarHeight * (maxScrollBarHeight / s_logsHeight ); 

            if (scrollBarHeight <= maxScrollBarHeight)
            {
                float scrollBarX = logsWindowRectangle.Right - ScrollBarWidth - 2f;
                float scrollBarY = logsWindowBottom - scrollBarHeight
                    - (maxScrollBarHeight - scrollBarHeight) * s_scrollValue - TextPadding;

                var scrollBarRectangle = new Rectangle(scrollBarX, scrollBarY,
                    ScrollBarWidth, scrollBarHeight);

                Graphics.FillRectangle(scrollBarRectangle, MainColor, Depth);
            }

            Graphics.Scissor = logsWindowRectangle;

            for (int i = 0; i < MaxLogs; i++)
            {
                if (s_logs[i] is null)
                    break;

                ConsoleLog log = s_logs[i].Value;

                string logText = log.Text;
                float logHeight = log.Height;

                float logY = logsBottom - currentLogsHeight - logHeight
                    - TextPadding;

                if (logY + logHeight < logsWindowRectangle.Top)
                    break;

                if (logY <= logsWindowBottom)
                {
                    string sign = GetLogSign(log.Direction);

                    s_logsFont.Draw(sign, new Vector2(inputX, logY), MainColor, Depth);
                    s_logsFont.Draw(logText, new Vector2(logsTextX, logY), log.Color, Depth);
                }

                currentLogsHeight += logHeight + TextPadding;
            }

            Graphics.ResetScissor();
        }


        public static void AddCommand(string name, Argument[] arguments, Action<Arguments> action)
        {
            name = name.ToLowerInvariant();
            var command = new ActionCommand(name, arguments, action);

            AddCommand(command);
        }

        public static void AddCommandContainer(Type type)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                CommandAttribute attribute = 
                    method.GetCustomAttribute<CommandAttribute>();

                if (attribute is null)
                    continue;

                string name = attribute.Name
                    .ToLowerInvariant();

                var command = new MethodCommand(name, method);

                AddCommand(command);
            }
        }

        public static void Log(object value, Color color)
        {
            LogOutput(value.ToString(), color);
        }

        public static void Log(object value)
        {
            LogOutput(value.ToString(), MainColor);
        }

        public static void Log(object value, LogType type)
        {
            LogPrefix prefix = s_logPrefixes[type];

            s_builder.Clear();

            s_builder.Append(VerticalLine);
            s_builder.Append(prefix.Text);
            s_builder.Append(VerticalLine);
            s_builder.Append(Space);
            s_builder.Append(value);

            string composedMessage = s_builder.ToString();

            Log(composedMessage, prefix.Color);
        }

        private static void LogInput(string message, Color color)
        {
            AddLog(message, LogDirection.Input, color);
        }

        private static void LogOutput(string message, Color color)
        {
            AddLog(message, LogDirection.Output, color);
        }

        private static void AddLog(string message, LogDirection direction, Color color)
        {
            if (message is null)
                message = NullStringMessage;
            else if (string.IsNullOrEmpty(message))
                message = EmptyStringMessage;

            float height = GetLogHeight(message);
            var log = new ConsoleLog(message, direction, height, color);

            int lastIndex = MaxLogs - 1;
            ConsoleLog? lastLog = s_logs[lastIndex];

            if (lastLog is not null)
            {
                ConsoleLog lastLogValue = lastLog.Value;
                s_logsHeight -= lastLogValue.Height + TextPadding;
            }

            for (int i = lastIndex; i >= 1; i--)
                s_logs[i] = s_logs[i - 1];

            s_logs[0] = log;
            s_logsHeight += height + TextPadding;
        }

        public static void Clear()
        {
            for (int i = 0; i < MaxLogs; i++)
                s_logs[i] = null;

            s_logsHeight = 0f;
        }

        public static void Run(ReadOnlySpan<char> input)
        {
            Preprocess(input, s_temporaryCommandBuffer, out int count);
            Run(s_temporaryCommandBuffer, count);

            FreeElementBuffers(s_temporaryCommandBuffer, count);
        }

        private static void AddCommand(Command command)
        {
            string name = command.Name;
            string error = command.Error;
            
            if (error is not null)
            {
                Log(error, LogType.Error);

                return;
            } 
            else if (s_commands.ContainsKey(name))
            {
                Log($"Command with name {name} already exists", LogType.Error);

                return;
            }

            s_commands[name] = command;
        }

        private static void Run(PreprocessedCommand[] commands, int count)
        {
            for (int i = 0; i < count; i++)
            {
                PreprocessedCommand command = commands[i];

                Run(command);
            }
        }

        private static void Run(PreprocessedCommand preprocessedCommand)
        {
            string error = preprocessedCommand.Error;

            if (error is not null)
            {
                Log(error, LogType.Error);

                return;
            }

            Element[] elements = preprocessedCommand.Elements;
            int elementsCount = preprocessedCommand.ElementsCount;

            string commandName = elements[0].String.ToLowerInvariant();

            if (s_commands.TryGetValue(commandName, out Command command))
            {
                string[] input = new string[elementsCount - 1];

                for (int i = 1; i < elementsCount; i++)
                    input[i - 1] = elements[i].String;

                command.Run(input, CatchRuntimeErrors);

                return;
            }

            Log($"Command \"{commandName}\" not found", LogType.Error);
        }

        private static void SplitArguments(ReadOnlySpan<char> input, Element[] buffer,
            out int count, out string error)
        {
            ReadOnlySpan<char> trimmed = MemoryExtensions.Trim(input);

            bool insideQuotes = false;
            bool previousIsSpace = false;
            bool isString = false;
            int length = trimmed.Length;

            count = 0;

            s_builder.Clear();

            for (int i = 0; i < length; i++)
            {
                char character = trimmed[i];

                if (character == Space)
                {
                    if (!insideQuotes)
                    {
                        if (!previousIsSpace)
                        {
                            AddArgument(s_builder, buffer, ref count,
                                ref isString);

                            s_builder.Clear();
                        }
                    }
                    else
                    {
                        s_builder.Append(Space);
                    }

                    previousIsSpace = true;

                    continue;
                }

                bool lastCharacter = i >= length - 1;

                if (character == BackSlash)
                {
                    if (!lastCharacter && trimmed[i + 1] == Quotes)
                    {
                        s_builder.Append(Quotes);

                        i += 1;
                    }
                }
                else if (character == Quotes)
                {
                    if (!insideQuotes)
                    {
                        if (previousIsSpace)
                        {
                            insideQuotes = true;
                            isString = true;
                        }
                        else
                        {
                            error = UnexpectedQuotesError;

                            goto exitWithError;
                        }
                    }
                    else
                    {
                        if (!lastCharacter && trimmed[i + 1] != Space)
                        {
                            error = UnexpectedQuotesError;

                            goto exitWithError;
                        }

                        insideQuotes = false;
                    } 
                }
                else
                {
                    s_builder.Append(character);
                }

                previousIsSpace = false;
            }

            if (insideQuotes)
            {
                error = UnclosedQuotesError;

                goto exitWithError;
            }

            if (s_builder.Length > 0 || (length > 0 && trimmed[length - 1] == Quotes))
                AddArgument(s_builder, buffer, ref count, ref isString);

            error = null;

            return;

        exitWithError:
            count = 0;
        }

        private static void AddArgument(StringBuilder builder, Element[] buffer, 
            ref int count, ref bool isString)
        {
            string initialString = builder.ToString();
            string replacement = initialString;

            if (!isString && s_aliases.TryGetValue(initialString, out string value))
                replacement = value;

            var element = new Element(replacement, initialString);

            buffer[count] = element;
            count++;

            isString = false;
        }

        private static void Preprocess(ReadOnlySpan<char> input, PreprocessedCommand[] buffer, 
            out int commandsCount)
        {
            int length = input.Length;
            bool insideQuotes = false;
            int commandStartIndex = 0;

            commandsCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (input[i] == Quotes && (i < 1 || input[i - 1] != BackSlash))
                {
                    insideQuotes = !insideQuotes;

                    continue;
                }

                if (!insideQuotes && input[i] == SemiColon)
                {
                    int distance = i - commandStartIndex;

                    if (distance > 0)
                        AddPreprocessedCommand(input, buffer, commandStartIndex, distance,
                            ref commandsCount);

                    commandStartIndex = i + 1;
                }
            }

            int finalDistance = length - commandStartIndex;

            if (finalDistance > 0)
                AddPreprocessedCommand(input, buffer, commandStartIndex, finalDistance,
                    ref commandsCount);
        }

        private static void AddPreprocessedCommand(ReadOnlySpan<char> input, PreprocessedCommand[] buffer,
            int startIndex, int length, ref int commandsCount)
        {
            ReadOnlySpan<char> command = input.Slice(startIndex, length);
            Element[] elementBuffer = AcquireElementBuffer();

            SplitArguments(command, elementBuffer, out int count, out string error);

            var preprocessed = new PreprocessedCommand(elementBuffer, count, error, 
                startIndex);

            buffer[commandsCount] = preprocessed;
            commandsCount++;
        }

        private static AutoCompleteData GenerateAutoComplete(string inputString,
            PreprocessedCommand[] commands, int commandsCount)
        {
            AutoCompleteData empty = AutoCompleteData.Empty;

            int inputLength = inputString.Length;
            char lastCharacter;

            if (inputLength > 0)
            {
                lastCharacter = inputString[inputLength - 1];

                if (lastCharacter == SemiColon)
                    return empty;
            }
            else
            {
                return empty;
            } 

            if (commandsCount < 1)
                return empty;

            PreprocessedCommand lastCommand = commands[commandsCount - 1];

            if (lastCommand.Error is not null)
                return empty;
             
            int elementsCount = lastCommand.ElementsCount;

            if (elementsCount < 1)
                return empty;

            Element[] elements = lastCommand.Elements;
            Element firstElement = elements[0];

            string firstElementString = firstElement.String;
            Span<char> firstElementLower = stackalloc char[firstElementString.Length];

            MemoryExtensions.ToLowerInvariant(firstElementString, firstElementLower);

            foreach (Command command in s_commands.Values)
            {
                s_builder.Clear();

                ImmutableArray<Argument> arguments = command.Arguments;
                int argumentsCount = arguments.Length;

                string commandName = command.Name;
                bool lastIsSpace = lastCharacter == Space;

                if (elementsCount == 1 && !lastIsSpace && commandName.StartsWith(firstElementLower) ||
                    commandName == firstElementString)
                {
                    string lastElement = null;

                    int lastIndex = elementsCount - 1;
                    Element element = elements[lastIndex];
                    string elementString = element.InitialString;
                    int length = elementString.Length;

                    if (lastIndex > argumentsCount)
                    {
                        return empty;
                    }
                    else if (elementsCount == 1)
                    {
                        lastElement = commandName;
                    }
                    else
                    {
                        Argument argument = arguments[lastIndex - 1];
                        lastElement = argument.AutoComplete(elementString);
                    }

                    if (lastElement.Length > length && !lastIsSpace)
                    {
                        lastElement = lastElement.Substring(length,
                            lastElement.Length - length);

                        s_builder.Append(lastElement);
                    }
                    else
                    {
                        lastElement = string.Empty;
                    } 

                    if (!lastIsSpace)
                        s_builder.Append(Space);

                    for (int i = lastIndex; i < argumentsCount; i++)
                    {
                        Argument argument = arguments[i];

                        s_builder.Append(argument.ToString());
                        s_builder.Append(Space);
                    }

                    string fullString = s_builder.ToString();

                    return new AutoCompleteData(fullString, lastElement);
                }
            }

            return empty;
        }

        private static void FindPossibleCommands(List<string> list,
            string inputString, PreprocessedCommand[] commands, int count)
        {
            list.Clear();

            int inputLength = inputString.Length;

            if (inputLength < 1)
                return;

            if (s_inputString[inputLength - 1] == Space)
                return;

            if (count < 1)
                return;

            PreprocessedCommand lastCommand = commands[count - 1];

            if (lastCommand.ElementsCount != 1)
                return;

            Element commandElement = lastCommand.Elements[0];
            string elementString = commandElement.InitialString;
            Span<char> lower = stackalloc char[elementString.Length];

            MemoryExtensions.ToLowerInvariant(elementString, lower);

            foreach (Command command in s_commands.Values)
            {
                string name = command.Name;

                if (name.StartsWith(lower))
                    list.Add(name);
            }
        }

        private static void ClearInput()
        {
            s_inputString = string.Empty; 
            s_autoCompleteString = string.Empty;
            s_autoCompleteElement = string.Empty;
            
            s_carriageIndex = 0;
            s_inputViewOffset = 0f;

            FreeElementBuffers(s_preprocessedCommands, s_commandsCount);

            s_possibleCommands.Clear();
            s_possibleCommandIndex = DefaultPossibleCommandIndex;
        }

        private static void SetInputText(string text)
        {
            s_inputString = text;
            s_carriageIndex = text.Length;   
            s_inputViewOffset = 0f;
        } 

        private static string GetLogSign(LogDirection direction)
        {
            if (direction == LogDirection.Input)
                return InputSign;

            return OutputSign;
        }

        private static Element[] AcquireElementBuffer()
        {
            if (s_elementBufferCache.TryPop(out Element[] buffer))
                return buffer;

            return new Element[MaxElements];
        }

        private static void FreeElementBuffer(Element[] buffer)
        {
            s_elementBufferCache.Push(buffer);
        }

        private static void FreeElementBuffers(PreprocessedCommand[] commands, int count)
        {
            for (int i = 0; i < count; i++)
            {
                PreprocessedCommand command = commands[i];

                FreeElementBuffer(command.Elements);
            }
        }

        private static float CalculateLogsOffsetX()
        {
            return s_logsFont.MeasureX(OutputSign) + TextPadding;
        }

        private static void SetLogsFontMaxWidth()
        {
            float logsOffsetX = CalculateLogsOffsetX();
            float logsWidth = Size.X - logsOffsetX - TextPadding * 3f - ScrollBarWidth;

            s_logsFont.MaxWidth = logsWidth;
        }

        private static void RecalculateLogsHeight()
        {
            s_logsHeight = 0f;

            SetLogsFontMaxWidth();

            for (int i = 0; i < MaxLogs; i++)
            {
                ConsoleLog? log = s_logs[i];

                if (log is null)
                    break;

                ConsoleLog value = log.Value;
                float height = GetLogHeight(value.Text);

                s_logs[i] = new ConsoleLog(value.Text, value.Direction,
                    height, value.Color);

                s_logsHeight += height + TextPadding;
            }
        }

        private static float GetLogHeight(string text)
        {
            float height = s_logsFont.MeasureY(text);
            height = Math.Max(height, s_logsFont.LineHeight);

            return height;
        }

        private enum LogDirection
        {
            Input,

            Output
        }

        private readonly struct AutoCompleteData
        {
            public static readonly AutoCompleteData Empty = new(string.Empty, string.Empty);

            public AutoCompleteData(string fullString, string lastElement)
            {
                FullString = fullString;
                LastElement = lastElement;
            }

            public string FullString { get; private init; }
            public string LastElement { get; private init; }
        }
        
        private readonly record struct Element(string String, string InitialString);

        private readonly record struct PreprocessedCommand(Element[] Elements, int ElementsCount, string Error, int StartIndex);

        private readonly record struct LogPrefix(string Text, Color Color);

        private readonly record struct ConsoleLog(string Text, LogDirection Direction, float Height, Color Color);
    }
}
