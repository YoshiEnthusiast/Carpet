using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class FontFamilyCache : Cache<FontFamily>
    {
        private readonly string _dataFileExtension = ".xml";

        public FontFamilyCache(string mainDirectory, bool load) : base(mainDirectory, load)
        {
            Extension = ".png";
        }

        protected override FontFamily CreateItem(string path)
        {
            string dataFileName = Path.ChangeExtension(path, _dataFileExtension);

            return new FontFamily(path, dataFileName);
        }
    }
}
