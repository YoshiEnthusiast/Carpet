using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public abstract class FileCache<T> : Cache<T>
    {
        public FileCache(string extension, string mainDirectory, bool load) : base(extension, mainDirectory, load) 
        {

        }

        protected abstract T CreateItem(Stream stream);

        protected override T CreateItem(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                if (Path.GetExtension(path) == Content.EncodedFileExtension)
                    using (MemoryStream stream = ContentEncoder.Decode(fileStream))
                        return CreateItem(stream);

                return CreateItem(fileStream);
            }
        }

        protected override string AddExtension(string fileName)
        {
            return fileName + Extension;
        }

        protected override bool IsValidExtension(string extension)
        {
            return base.IsValidExtension(extension) || extension == Content.EncodedFileExtension;
        }
    }
}
