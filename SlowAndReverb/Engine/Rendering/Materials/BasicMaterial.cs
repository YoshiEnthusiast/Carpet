namespace SlowAndReverb
{
    public class BasicMaterial : Material
    {
        public BasicMaterial()
        {
            ShaderProgram = Content.GetShaderProgram(Content.DefaultShaderName);
        }
    }
}
