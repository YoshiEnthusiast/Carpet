using System;
using System.Collections.Generic;
using System.Reflection;

namespace Carpet
{
    public class Editor
    {
        private static readonly List<Type> s_entityTypes = [];
        private static readonly List<string> s_entityNamesToLower = [];
        private static readonly Dictionary<string, Type> s_entityTypeByName = [];

        public static IEnumerable<Type> EntityTypes => s_entityTypes;
        public static IEnumerable<string> EntityNamesToLower => s_entityNamesToLower;   

        internal static void Initialize()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            foreach (Type type in currentAssembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Entity)) || type.IsAbstract)
                    continue;

                string name = type.Name.ToLower();

                s_entityTypes.Add(type);
                s_entityTypeByName[name] = type;
            }
        }

        public static Entity CreateEntityOfType(Type type)
        {
            return CreateEntityOfType(type, 0f, 0f);
        }

        public static Entity CreateEntityOfType(Type type, float x, float y)
        {
            return (Entity)Activator.CreateInstance(type, new object[] 
            { 
                x, 
                y 
            });   
        }

        public static Type GetEntityTypeByName(string name)
        {
            if (s_entityTypeByName.TryGetValue(name, out Type type))
                return type;

            return default;
        }
    }
}
