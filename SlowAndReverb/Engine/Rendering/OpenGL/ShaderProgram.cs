using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public sealed class ShaderProgram : OpenGLObject
    {
        private readonly Uniform[] _uniforms;
        private readonly int _maxUniformCharacters = 100;

        public ShaderProgram(string vertexSource, string fragmentSource, string geometrySource)
        {
            Handle = GL.CreateProgram();
            bool geometrySourceExists = geometrySource is not null;

            int vertexShader = CreateShader(vertexSource, ShaderType.VertexShader);
            int fragmentShader = CreateShader(fragmentSource, ShaderType.FragmentShader);
            int geometryShader = 0;

            if (geometrySourceExists)
                geometryShader = CreateShader(geometrySource, ShaderType.GeometryShader);

            GL.LinkProgram(Handle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            if (geometrySourceExists)
                GL.DeleteShader(geometryShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int count);
            _uniforms = new Uniform[count];

            for (int i = 0; i < count; i++)
            {
                GL.GetActiveUniform(Handle, i, _maxUniformCharacters, out _, out _, out ActiveUniformType type, out string name);

                int bracketsIndex = name.IndexOf('[');

                if (bracketsIndex >= 0)
                    name = name.Substring(0, bracketsIndex);

                int location = GL.GetUniformLocation(Handle, name);

                _uniforms[i] = new Uniform(name, type, location);
            }
        }

        public ShaderProgram(string vertexSource, string fragmentSource) : this(vertexSource, fragmentSource, null)
        {

        }

        public bool TryGetUniformByName(string name, out Uniform result)
        {
            foreach (Uniform uniform in _uniforms)
            {
                if (uniform.Name == name)
                {
                    result = uniform;

                    return true;
                }
            }

            result = default(Uniform);

            return false;
        }

        public IEnumerable<Uniform> GetUniformsOfType(ActiveUniformType type)
        {
            foreach (Uniform uniform in _uniforms)  
                if (uniform.Type == type)
                    yield return uniform;   
        }

        public void SetUniform(string name, int value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, float value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, float value1, float value2)
        {
            SetUniform(GetUniformLocation(name), value1, value2);
        }

        public void SetUniform(string name, Vector2GL value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Vector2 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, float value1, float value2, float value3)
        {
            SetUniform(GetUniformLocation(name), value1, value2, value3);
        }

        public void SetUniform(string name, Vector3 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, float value1, float value2, float value3, float value4)
        {
            SetUniform(GetUniformLocation(name), value1, value2, value3, value4);
        }

        public void SetUniform(string name, Vector4 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Color value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix2 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix2x3 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix2x4 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix3 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix3x2 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix3x4 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix4 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix4x2 value)
        {
            SetUniform(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, int length, int[] values)
        {
            SetUniform(GetUniformLocation(name), length, values);
        }

        public void SetUniform(int location, int value)
        {
            GL.Uniform1(location, value);
        }

        public void SetUniform(int location, int length, int[] values)
        {
            GL.Uniform1(location, length, values);
        }

        public void SetUniform(int location, float value)
        {
            GL.Uniform1(location, value);
        }

        public void SetUniform(int location, float value1, float value2)
        {
            GL.Uniform2(location, value1, value2);
        }

        public void SetUniform(int location, Vector2GL value)
        {
            GL.Uniform2(location, value);
        }

        public void SetUniform(int location, Vector2 value)
        {
            SetUniform(location, value.ToVector2GL());
        }

        public void SetUniform(int location, float value1, float value2, float value3)
        {
            GL.Uniform3(location, value1, value2, value3);
        }

        public void SetUniform(int location, Vector3 value)
        {
            GL.Uniform3(location, value);
        }

        public void SetUniform(int location, float value1, float value2, float value3, float value4)
        {
            GL.Uniform4(location, value1, value2, value3, value4);
        }

        public void SetUniform(int location, Vector4 value)
        {
            GL.Uniform4(location, value);
        }

        public void SetUniform(int location, Color value)
        {
            SetUniform(location, value.ToVector4());
        }

        public void SetUniform(int location, Matrix2 value)
        {
            GL.UniformMatrix2(location, true, ref value);
        }

        public void SetUniform(int location, Matrix2x3 value)
        {
            GL.UniformMatrix2x3(location, true, ref value);
        }

        public void SetUniform(int location, Matrix2x4 value)
        {
            GL.UniformMatrix2x4(location, true, ref value);
        }

        public void SetUniform(int location, Matrix3 value)
        {
            GL.UniformMatrix3(location, true, ref value);
        }

        public void SetUniform(int location, Matrix3x2 value)
        {
            GL.UniformMatrix3x2(location, true, ref value);
        }

        public void SetUniform(int location, Matrix3x4 value)
        {
            GL.UniformMatrix3x4(location, true, ref value);
        }

        public void SetUniform(int location, Matrix4 value)
        {
            GL.UniformMatrix4(location, true, ref value);
        }

        public void SetUniform(int location, Matrix4x2 value)
        {
            GL.UniformMatrix4x2(location, true, ref value);
        }

        public void SetUniform(int location, Matrix4x3 value)
        {
            GL.UniformMatrix4x3(location, true, ref value);
        }

        protected override void Bind(int handle)
        {
            GL.UseProgram(handle);
        }

        private int CreateShader(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            Console.WriteLine(GL.GetShaderInfoLog(shader));

            GL.AttachShader(Handle, shader);

            return shader;
        }

        private int GetUniformLocation(string name)
        {
            if (TryGetUniformByName(name, out Uniform uniform))
                return uniform.Location;

            return -1;
        }
    }
}
