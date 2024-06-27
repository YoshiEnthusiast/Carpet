using OpenTK.Graphics.OpenGL;
using System;

namespace Carpet
{
    public class RayTracer
    {
        private static readonly JumpFloodComputer s_jumpFloodComputer = new();
        private static readonly DistanceFieldComputer s_distanceFieldComputer = new();
        private static readonly JumpFloodSeedComputer  s_jumpFloodSeedComputer = new();
        
        private static readonly RayMarchingMaterial s_rayMarchingMaterial = new();

        private static Texture2D s_voronoiInput;
        private static Texture2D s_voronoiOutput;
        private static Texture2D s_occludersEmitters;
        public static Texture2D s_lightMap;

        public static void Setup()
        {
            s_voronoiInput = Texture2D.CreateEmpty(320, 180);
            s_voronoiOutput = Texture2D.CreateEmpty(320, 180);
            s_occludersEmitters = Texture2D.CreateEmpty(320, 180);
            s_lightMap = Texture2D.CreateEmpty(320, 180);

            s_rayMarchingMaterial.Textures.Add(s_occludersEmitters);
        } 

        public static void Draw()
        {
            // HACK: I'm currently displaying the lightmap on Console layer
            
            
            
            
            // WARNING: Draw occluder and emitters
            SpriteBatch batch = Graphics.SpriteBatch;
            RenderTarget target = RenderTarget.FromTexture(s_occludersEmitters);
            batch.Begin(target, BlendMode.Opaque, Color.Transparent, null);

            // Graphics.FillCircle(Carpet.ConsoleLayer.MousePosition, Color.Yellow, 20, 0f);
            Graphics.FillCircle(new Vector2(50f, 80f), Color.SkyBlue, 10, 0f);
            // Graphics.DrawRectangle(new Rectangle(110f, 80f, 40f, 40f), Color.White, 0f);


            Graphics.FillCircle(new Vector2(40, 40), Color.Black, 20, 0f);
            Graphics.FillCircle(new Vector2(250, 150), Color.Black, 20, 0f);
            Graphics.FillCircle(new Vector2(50, 120), Color.Black, 20, 0f);

            batch.End();







            // WARNING: Generate jump flood seed


            GL.BindImageTexture(0, s_occludersEmitters.Handle, 0, false, 0, 
                TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(1, s_voronoiOutput.Handle, 0, false, 0,
                TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
            s_jumpFloodSeedComputer.Compute(320, 180, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            // target = RenderTarget.FromTexture(s_voronoiOutput);
            // batch.Begin(target, BlendMode.Opaque, Color.Transparent, null);
            //
            // Graphics.Draw(s_occludersEmitters, s_seedMaterial, 
            //         new Rectangle(0, 0, 320, 180), Color.White, 0f);
            //
            // batch.End();










            // WARNING: Perform jump flood algorithm
            int maxSide = Math.Max(320, 180);
            int rounds = (int)Math.Log2(maxSide);

            for (int i = 0; i < rounds; i++)
            {
                int stepLength = maxSide / (int)Math.Pow(2, i + 1);

                Texture2D temp = s_voronoiInput;
                s_voronoiInput = s_voronoiOutput;
                s_voronoiOutput = temp;

                GL.BindImageTexture(0, s_voronoiInput.Handle, 0, false, 0, 
                    TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(1, s_voronoiOutput.Handle, 0, false, 0,
                    TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                s_jumpFloodComputer.StepLength = stepLength;
                s_jumpFloodComputer.Compute(320, 180, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            }

            Texture2D distanceField = s_voronoiOutput;








            // WARNING: Generate distance field
            GL.BindImageTexture(0, distanceField.Handle, 0, false, 0,
                TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            s_distanceFieldComputer.Compute(320, 180, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);











            // WARNING: Ray march
            target = RenderTarget.FromTexture(s_lightMap);
            batch.Begin(target, BlendMode.Opaque, Color.Transparent, null);

            s_rayMarchingMaterial.Time = Engine.TimeElapsedFloat;

            Graphics.Draw(distanceField, s_rayMarchingMaterial, 
                    new Rectangle(0, 0, 320, 180), Color.White, 0f);

            batch.End();
        }

        private class JumpFloodSeedComputer : Computer 
        {
            public JumpFloodSeedComputer()
            {
                ShaderProgram = Content.GetComputeShaderProgram("jumpFloodSeed");
            }
        }

        private class JumpFloodComputer : Computer
        {
            public JumpFloodComputer()
            {
                ShaderProgram = Content.GetComputeShaderProgram("jumpFlood");
            }

            [Uniform("u_StepLength")] public int StepLength { get; set; }
        }

        private class DistanceFieldComputer : Computer
        {
            public DistanceFieldComputer()
            {
                ShaderProgram = Content.GetComputeShaderProgram("distanceField");
            }
        }
    }
}
