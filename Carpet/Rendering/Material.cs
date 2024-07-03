using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public abstract class Material : ShaderProgramWrapper
    {
        public Material()
        {
            Textures = new Texture2D[OpenGL.MaxTextureUnits - 1];
        }

        public PipelineShaderProgram ShaderProgram { get; protected init; }
        public int ExtraTexturesCount => GetExtraTexturesCount();

        public Texture[] Textures { get; private init; }

        protected  override ShaderProgram Program => ShaderProgram;

        public void Apply()
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                Texture texture = Textures[i];

                if (texture is null)
                    break;

                texture.Bind(TextureUnit.Texture0 + i);
            }

            SetUniforms();
        }

        public void Unapply()
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                Texture texture = Textures[i];

                if (texture is null)
                    return;

                texture.Unbind();
            }
        }
        
        public int GetExtraTexturesCount()
        {
            int count = 0;

            for (int i = 0; i < Textures.Length; i++)
            {
                if (Textures[i] is null)
                    break;

                count++;
            }

            return count;
        }
    }
}
