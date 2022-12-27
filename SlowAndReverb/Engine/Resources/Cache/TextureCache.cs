using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    internal class TextureCache : FileCache<Texture>
    {
        public TextureCache(string mainDirectory, bool load) : base(".png", mainDirectory, load)
        {

        }

        protected override Texture CreateItem(Stream stream)
        {
            return Texture.FromStream(stream);  
        }
    }
}
