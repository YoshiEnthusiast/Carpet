using System;
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
}
