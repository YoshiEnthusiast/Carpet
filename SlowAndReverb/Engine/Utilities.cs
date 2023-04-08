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

        public static Rectangle RectangleFromCircle(Vector2 position, float radius)
        {
            var radiusVector = new Vector2(radius);

            return new Rectangle(position - radiusVector, position + radiusVector);
        }
    }
}
