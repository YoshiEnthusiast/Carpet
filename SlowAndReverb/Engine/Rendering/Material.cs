using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SlowAndReverb
{
    public abstract class Material
    {
        private readonly List<Texture> _textures = new List<Texture>();

        private static readonly Dictionary<Type, IEnumerable<UniformStorage>> s_uniformStorages = new Dictionary<Type, IEnumerable<UniformStorage>>();
        private static readonly Dictionary<Type, MethodInfo> s_setUniformMethods = new Dictionary<Type, MethodInfo>();

        private static readonly Dictionary<Type, ActiveUniformType> s_uniformTypes = new Dictionary<Type, ActiveUniformType>()
        {
            [typeof(int)] = ActiveUniformType.Int,
            [typeof(float)] = ActiveUniformType.Float,
            [typeof(Vector2)] = ActiveUniformType.FloatVec2,
            [typeof(Vector3)] = ActiveUniformType.FloatVec3,
            [typeof(Vector4)] = ActiveUniformType.FloatVec4,
            [typeof(Color)] = ActiveUniformType.FloatVec4,
            [typeof(Matrix2)] = ActiveUniformType.FloatMat2,
            [typeof(Matrix3)] = ActiveUniformType.FloatMat3,
            [typeof(Matrix4)] = ActiveUniformType.FloatMat4,
            [typeof(Matrix2x3)] = ActiveUniformType.FloatMat2x3,
            [typeof(Matrix2x4)] = ActiveUniformType.FloatMat2x4,
            [typeof(Matrix3x2)] = ActiveUniformType.FloatMat3x2,
            [typeof(Matrix4x2)] = ActiveUniformType.FloatMat4x2,
            [typeof(Matrix3x4)] = ActiveUniformType.FloatMat3x4,
        };

        private bool _applied;

        public Material()
        {

        }

        public ShaderProgram ShaderProgram { get; protected init; }

        public int ExtraTexturesCount => GetExtraTexturesCount();

        protected IList<Texture> Textures
        {
            get
            {
                if (_applied)
                    return null;

                return _textures;
            }
        }

        internal static void InitializeUniforms()
        {
            s_uniformStorages.Clear();
            s_setUniformMethods.Clear();

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types.Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Material))))
                s_uniformStorages.Add(type, GetUniformStoragesForType(type));

            Type shaderProgramType = typeof(ShaderProgram);

            foreach (Type type in s_uniformTypes.Keys)
            {
                MethodInfo method = shaderProgramType.GetMethod("SetUniform", BindingFlags.Instance | BindingFlags.Public, new Type[]
                {
                    typeof(int),
                    type
                });

                s_setUniformMethods.Add(type, method);
            }
        }

        public void Apply()
        {
            for (int i = 0; i < GetExtraTexturesCount(); i++)
            {
                Texture texture = _textures[i];

                texture.Bind(TextureUnit.Texture0 + i);
            }

            foreach (UniformStorage storage in s_uniformStorages[GetType()])
            {
                if (!ShaderProgram.TryGetUniformByName(storage.Name, out Uniform uniform))
                    continue;

                PropertyInfo property = storage.Property;
                Type propertyType = property.PropertyType;

                if (uniform.Type != s_uniformTypes[propertyType])
                    continue;

                int location = uniform.Location;
                object value = property.GetValue(this);

                s_setUniformMethods[propertyType].Invoke(ShaderProgram, new object[]
                {
                    location,
                    value
                });
            }
        }

        public void Unapply()
        {
            for (int i = 0; i < GetExtraTexturesCount(); i++)
            {
                Texture texture = _textures[i];

                texture.Unbind();
            }
        }

        private static IEnumerable<UniformStorage> GetUniformStoragesForType(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!s_uniformTypes.ContainsKey(property.PropertyType) || property.GetGetMethod() is null)
                    continue;

                UniformAttribute attribute = property.GetCustomAttribute<UniformAttribute>();

                if (attribute is null)
                    continue;

                yield return new UniformStorage(attribute.Name, property);
            }
        }

        private int GetExtraTexturesCount()
        {
            return Maths.Min(_textures.Count, OpenGL.MaxTextureUnits - 1);
        }
    }
}
