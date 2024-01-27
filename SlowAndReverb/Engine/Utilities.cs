using OpenTK.Mathematics;
using System.IO;
using System.Xml;

namespace Carpet
{
    public static class Utilities
    {
        public static XmlDocument LoadXML(string fileName)
        {
            var document = new XmlDocument();

            using (FileStream stream = File.OpenRead(fileName))
                document.Load(stream);

            return document;
        }
    }
}
