using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
