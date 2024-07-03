using System.IO;

namespace Carpet
{
    internal class TextureCache : FileCache<Texture2D>
    {
        public TextureCache(string mainDirectory, bool load) : base(".png", mainDirectory, load)
        {

        }

        protected override Texture2D CreateItem(Stream stream)
        {
            return Texture2D.FromStream(stream);  
        }
    }
}
