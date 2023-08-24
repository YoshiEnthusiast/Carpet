using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    internal sealed class Program
    {
        private static UntitledGame s_game;

        private abstract class Abstract<T>
        {
            public abstract T M();
        }

        private class B : Abstract<string>
        {
            public override string M()
            {
                return "";
            }
        }   

        private record struct Boid(Vector2 Position, Vector2 Velocity);


        public record class I(int A, int B);


        private static void Main(string[] args)
        {
            //var s = Std430LayoutItem.StructArray<Boid, Vector2>(20);
            //var f = Std430LayoutItem.Struct<Boid, Vector2>();
            //Console.WriteLine(s.BaseAlignment + " " + f.BaseAlignment);

            //return;

            //LAB l1 = new LAB(1f, 0f, 5f);
            //LAB l2 = new LAB(2f, 5f, 0f);

            //Console.WriteLine(GetDistance(l1, l2));

            //return;

            //var a = new Abstract<string>[5];

            //var u = new UniformBuffer();
            //u.Initialize(new UniformBlockItem[]
            //{
            //    UniformBlockItem.Scalar(),
            //    UniformBlockItem.Vector3(),

            //    UniformBlockItem.Matrix(4),

            //    UniformBlockItem.Array(3),

            //    UniformBlockItem.Scalar(),
            //    UniformBlockItem.Scalar()
            //});


            //string s = "2 = 1, 8 = 2, 10 = 3, 11 = 4, 16 = 5, 18 = 6, 22 = 7, 24 = 8, 26 = 9, 27 = 10, 30 = 11, 31 = 12, 64 = 13, 66 = 14, 72 = 15, 74 = 16, 75 = 17, 80 = 18, 82 = 19, 86 = 20, 88 = 21, 90 = 22, 91 = 23, 94 = 24, 95 = 25, 104 = 26, 106 = 27, 107 = 28, 120 = 29, 122 = 30, 123 = 31, 126 = 32, 127 = 33, 208 = 34, 210 = 35, 214 = 36, 216 = 37, 218 = 38, 219 = 39, 222 = 40, 223 = 41, 248 = 42, 250 = 43, 251 = 44, 254 = 45, 255 = 46, 0 = 47 ";
            //s = s.Replace(",", "");
            //s = s.Replace("=", "");

            //string[] a = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            //var d = new XmlDocument();
            //XmlElement t = d.CreateElement("Tiles");
            //d.AppendChild(t);

            //for (int i = 0; i < a.Length - 1; i += 2)
            //{
            //    XmlElement tile = d.CreateElement("Tile");

            //    tile.SetAttribute("Mask", a[i]);
            //    tile.SetAttribute("Frame", a[i + 1]);


            //    t.AppendChild(tile);
            //}

            //d.Save("tiles.xml");




            // temporary
            Resolution initialREsolution = Resolution.SupportedResolutions.First();

            s_game = new UntitledGame(60d, 60d, "Untitled game");

            s_game.Run(initialREsolution, TextureLoadMode.LoadAtlas);

            return;

            Resolution resolution = Resolution.SupportedResolutions.First();

            var settings = new GameWindowSettings()
            {
                UpdateFrequency = 60d,
                //RenderFrequency = 60d
            };

            var nativeSettings = new NativeWindowSettings()
            {
                Title = "Slow and reverb",
                WindowBorder = WindowBorder.Fixed,
                Size = new Vector2i(resolution.Width, resolution.Height)
            };

            var window = new OldDebugWindow(settings, nativeSettings);

            Resolution.Initialize(window);
            Resolution.SetCurrent(resolution);

            window.Run();
        }
    }
}
