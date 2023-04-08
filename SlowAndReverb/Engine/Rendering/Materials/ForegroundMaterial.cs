﻿namespace SlowAndReverb
{
    public class ForegroundMaterial : Material
    {
        public ForegroundMaterial()
        {
            ShaderProgram = Content.GetShaderProgram("foreground");

            Textures.Add(RenderTargets.LightMap.Texture);
        }

        [Uniform("u_Darkness")] public float Darkness { get; set; } = 0.1f;
    }
}
