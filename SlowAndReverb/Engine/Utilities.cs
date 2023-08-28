using OpenTK.Mathematics;
using System.IO;
using System.Xml;

namespace SlowAndReverb
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
