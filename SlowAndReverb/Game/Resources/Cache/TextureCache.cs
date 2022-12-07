using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    internal class TextureCache : Cache<Texture>
    {
        public TextureCache(string mainDirectory, bool load) : base(mainDirectory, load)
        {
            Extension = ".png";
        }

        protected override Texture CreateItem(string path)
        {
            return Texture.FromFile(path);
        }
    }
}
