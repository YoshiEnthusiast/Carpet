﻿using System;
using System.Xml;

namespace Carpet
{
    internal static class Extensions
    {
        public static int GetIntAttribute(this XmlElement element, string name)
        {
            return Convert.ToInt32(element.GetAttribute(name));
        }

        public static T GetEnum<T>(this XmlElement element, string name) where T : struct
        {
            string value = element.GetAttribute(name);

            return Enum.Parse<T>(value);
        }

        public static void SetAttribute(this XmlElement element, string name, object value)
        {
            element.SetAttribute(name, value.ToString());
        }

        public static bool StartsWith(this string line, Span<char> span)
        {
            return ((ReadOnlySpan<char>)line).StartsWith(span);
        }
    }
}
