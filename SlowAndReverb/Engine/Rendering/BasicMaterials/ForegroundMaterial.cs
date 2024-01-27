using System;

namespace Carpet
{
    public class ForegroundMaterial : Material
    {
        public ForegroundMaterial()
        {
            ShaderProgram = Content.GetPipelineShaderProgram("foreground");

            Textures.Add(Demo.Lightmap.Texture);
        }

        [Uniform("u_Darkness")] public float Darkness { get; set; } = 0.1f;

        public void SetPalette(Palette palette)
        {
            Texture3D texture = palette.Texture;

            // Have Texture[] Textures and int TextureCount instead
            // of a list so this doesn't happen

            if (Textures.Count >= 2)
            {
                Textures[1] = texture;

                return;
            }

            Textures.Add(texture);
        }
    }
}
