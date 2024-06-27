using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Carpet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using OpenTK.Core.Exceptions;
using System.Collections.Immutable;
using System.IO;
using OpenTK.Platform.Windows;
using OpenTK.Graphics.GL;

namespace Carpet.Platforming
{
    internal sealed class Program
    {
        private static Game s_game;

        private static void Main(string[] args)
        {
            Resolution initialResolution = Resolution.SupportedResolutions.First();

            s_game = new Game(60d, 60d, "Demo");
            s_game.Run(initialResolution, TextureLoadMode.LoadAtlas);
        }
    }
}
