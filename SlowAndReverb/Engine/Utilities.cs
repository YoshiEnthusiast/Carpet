using OpenTK.Mathematics;
using System.IO;
using System.Xml;

namespace SlowAndReverb
{
    public static class Utilities
    {
        public static Matrix4 CreateTransformMatrix(Vector2 position, Vector2 rotationOrigin, Vector2 scale, float angle)
        {
            float originX = rotationOrigin.RoundedX;
            float originY = rotationOrigin.RoundedY;   

            Matrix4 matrix = Matrix4.CreateScale(scale.X, scale.Y, 0f);

            if (angle != 0f)
                matrix *= Matrix4.CreateTranslation(-originX, -originY, 0f) * Matrix4.CreateRotationZ(angle) * Matrix4.CreateTranslation(originX, originY, 0f);

            matrix *= Matrix4.CreateTranslation(position.RoundedX - originX, position.RoundedY - originY, 0f);

            return matrix;
        }

        public static XmlDocument LoadXML(string fileName)
        {
            var document = new XmlDocument();

            using (FileStream stream = File.OpenRead(fileName))
                document.Load(stream);

            return document;
        }
    }
}
