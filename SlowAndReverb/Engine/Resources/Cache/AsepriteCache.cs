﻿using System.IO;

namespace SlowAndReverb
{
    public class AsepriteCache : FileCache<Aseprite>
    {
        public AsepriteCache(string mainDirectory, bool load) : base(".ase", mainDirectory, load)
        {

        }

        protected override Aseprite CreateItem(Stream stream)
        {
            return Aseprite.FromStream(stream);
        }
    }
}