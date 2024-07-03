using System.IO;

namespace Carpet
{
    public class FontFamilyCache : Cache<FontFamily>
    {
        public FontFamilyCache(string mainDirectory, bool load) : base(".xml", mainDirectory, load)
        {

        }

        protected override FontFamily CreateItem(string path)
        {
            string textureName = Path.GetFileNameWithoutExtension(Path.ChangeExtension(path, null));
            string texturePath = $@"Fonts/{textureName}";

            return new FontFamily(texturePath, path);
        }
    }
}
