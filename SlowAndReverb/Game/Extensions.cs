using System.Xml;

namespace SlowAndReverb
{
    internal static class Extensions
    {
        public static int GetIntAttribute(this XmlElement element, string name)
        {
            return Convert.ToInt32(element.GetAttribute(name));
        }
    }

    // Чтобы применять к текстуре слоя какие-то post processing эффекты и чтобы все оставалось pixel perfect, мне надо в шейдере фрагмента пикселезировать uv. 
}
