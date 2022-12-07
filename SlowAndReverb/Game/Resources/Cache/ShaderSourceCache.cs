using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class ShaderSourceCache : Cache<string>
    {
        public ShaderSourceCache(string mainDirectory, string extension, bool load) : base(mainDirectory, load)
        {
            Extension = extension;  
        }

        protected override string CreateItem(string path)
        {
            using (StreamReader reader = new StreamReader(path))
                return reader.ReadToEnd();
        }
    }
}
