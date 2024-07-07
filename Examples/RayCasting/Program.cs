namespace Carpet.Examples.RayCasting
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            Resolution initialResolution = new Resolution(1280, 720);

            var game = new Game(60d, 60d, "Ray Casting");
            game.Run(initialResolution, TextureLoadMode.SaveAtlas);
        }
    }
}
