﻿namespace Carpet.Platforming
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            Resolution initialResolution = new Resolution(1280, 720);

            var game = new Game(120d, 120d, "Platforming");
            game.Run(initialResolution, TextureLoadMode.LoadAtlas);
        }
    }
}
