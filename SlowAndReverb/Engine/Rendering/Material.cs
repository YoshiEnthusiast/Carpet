﻿using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Carpet
{
    public abstract class Material : ShaderProgramWrapper
    {
        private readonly List<Texture> _textures = [];

        private bool _applied;

        public PipelineShaderProgram ShaderProgram { get; protected init; }
        public int ExtraTexturesCount => GetExtraTexturesCount();

        // TODO: what the hell is this
        public IList<Texture> Textures
        {
            get
            {
                if (_applied)
                    return null;

                return _textures;
            }
        }

        protected  override ShaderProgram Program => ShaderProgram;

        public void Apply()
        {
            for (int i = 0; i < GetExtraTexturesCount(); i++)
            {
                Texture texture = _textures[i];

                texture.Bind(TextureUnit.Texture0 + i);
            }

            SetUniforms();
        }

        public void Unapply()
        {
            for (int i = 0; i < GetExtraTexturesCount(); i++)
            {
                Texture texture = _textures[i];

                texture.Unbind();
            }
        }

        private int GetExtraTexturesCount()
        {
            return Maths.Min(_textures.Count, OpenGL.MaxTextureUnits - 1);
        }
    }
}
