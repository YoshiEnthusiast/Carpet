using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Carpet
{
    public abstract class ShaderProgramWrapper
    {
        private static readonly Dictionary<Type, IEnumerable<UniformStorage>> s_uniformStorages = [];
        private static readonly Dictionary<Type, MethodInfo> s_setUniformMethods = [];

        private static readonly Dictionary<Type, ActiveUniformType> s_uniformTypes = new()
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

        protected abstract ShaderProgram Program { get; }

        internal static void InitializeUniforms()
        {
            s_uniformStorages.Clear();
            s_setUniformMethods.Clear();

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes();
            
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(typeof(ShaderProgramWrapper)))
                    continue;

                if (type.IsAbstract)
                    continue;

                s_uniformStorages.Add(type, GetUniformStoragesForType(type));
            }

            Type shaderProgramType = typeof(PipelineShaderProgram);

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

        protected void SetUniforms()
        {
            foreach (UniformStorage storage in s_uniformStorages[GetType()])
            {
                if (!Program.TryGetUniformByName(storage.Name, out Uniform uniform))
                    continue;

                PropertyInfo property = storage.Property;
                Type propertyType = property.PropertyType;

                if (uniform.Type != s_uniformTypes[propertyType])
                    continue;

                int location = uniform.Location;
                object value = property.GetValue(this);

                s_setUniformMethods[propertyType].Invoke(Program, new object[]
                {
                    location,
                    value
                });
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
    }
}
