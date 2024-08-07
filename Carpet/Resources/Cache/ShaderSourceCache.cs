﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class ShaderSourceCache : Cache<string>
    {
        public ShaderSourceCache(string mainDirectory, string extension, bool load) : base(extension, mainDirectory, load)
        {

        }

        protected override string CreateItem(string path)
        {
            using (StreamReader reader = new StreamReader(path))
                return reader.ReadToEnd();
        }
    }
}
