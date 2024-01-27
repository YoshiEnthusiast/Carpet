using System.IO;

namespace Carpet
{
    public class PaletteCache : FileCache<Palette>
    {
        public PaletteCache(string mainDirectory, bool load) : base(".png", mainDirectory, load)
        {
            
        }

        protected override Palette CreateItem(Stream stream)
        {
            return new Palette(stream);
        }
    }
}
