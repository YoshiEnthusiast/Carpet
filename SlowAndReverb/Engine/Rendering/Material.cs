using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public abstract class Material : ShaderProgramWrapper
    {
        private readonly List<Texture> _textures = new List<Texture>();

        private bool _applied;

        public PipelineShaderProgram ShaderProgram { get; protected init; }
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
