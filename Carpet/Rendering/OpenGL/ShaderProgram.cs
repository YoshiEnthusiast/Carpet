﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carpet
{
    public abstract class ShaderProgram : OpenGLObject
    {
        private const int MaxUniformNameCharacters = 100;

        private static readonly ActiveUniformType[] s_textureUniformTypes =
        {
            ActiveUniformType.Image1D,
            ActiveUniformType.Image2D,
            ActiveUniformType.Image3D,
            ActiveUniformType.ImageCube
        };

        private Uniform[] _uniforms;

        public ShaderProgram()
        {
            Handle = GL.CreateProgram();
        }

        public int TextureUniformsCount { get; private set; }

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

        public void SetUniform(string name, bool value)
        {
            SetUniform(GetUniformLocation(name), value);
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

        public void SetUniform(int location, bool value)
        {
            GL.Uniform1(location, Convert.ToSingle(value));
        }

        protected override void Bind(int handle)
        {
            GL.UseProgram(handle);
        }

        protected int CreateShader(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            string error = GL.GetShaderInfoLog(shader);

            if (!string.IsNullOrEmpty(error))
                Console.WriteLine(error);

            GL.AttachShader(Handle, shader);

            return shader;
        }

        protected void ScanUniforms()
        {
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int uniformsCount);
            _uniforms = new Uniform[uniformsCount];

            int textureUniformsCount = 0;

            for (int i = 0; i < uniformsCount; i++)
            {
                GL.GetActiveUniform(Handle, i, MaxUniformNameCharacters, out _, out _, out ActiveUniformType type, out string name);

                int bracketsIndex = name.IndexOf('[');

                if (bracketsIndex >= 0)
                    name = name.Substring(0, bracketsIndex);

                int location = GL.GetUniformLocation(Handle, name);

                if (s_textureUniformTypes.Contains(type))
                    textureUniformsCount++;

                _uniforms[i] = new Uniform(name, type, location);
            }

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniformBlocks, out int uniformBlocksCount);

            for (int i = 0; i < uniformBlocksCount; i++)
                GL.UniformBlockBinding(Handle, i, i);

            TextureUniformsCount = textureUniformsCount;
        }

        private int GetUniformLocation(string name)
        {
            if (TryGetUniformByName(name, out Uniform uniform))
                return uniform.Location;

            return -1;
        }
    }
}
