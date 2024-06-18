using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class RayTracer
    {
        private static readonly JumpFloodComputer s_jumpFloodComputer = new();
        private static readonly DistanceFieldComputer s_distanceFieldComputer = new();

        private static readonly JumpFloodSeedMaterial s_seedMaterial = new();

        private static Texture2D s_voronoiInput;
        private static Texture2D s_voronoiOutput;

        public static void Setup()
        {
            s_voronoiInput = Texture2D.CreateEmpty(320, 180);
            s_voronoiOutput = Texture2D.CreateEmpty(320, 180);
        } 

        public static void Draw()
        {
            SpriteBatch batch = Graphics.SpriteBatch;
            RenderTarget target = RenderTarget.FromTexture(s_voronoiOutput);

            batch.Begin(target, BlendMode.Opaque, Color.Transparent, null);

            s_seedMaterial.Resolution = new Vector2(320f, 180f);

            Graphics.FillRectangle(new Rectangle(10f, 10f, 40f, 40f),
               s_seedMaterial, Color.White, 0f);
            Graphics.FillRectangle(new Rectangle(100f, 80f, 30f, 20f), 
                s_seedMaterial, Color.White, 0f);
            Graphics.FillRectangle(new Rectangle(200f, 150f, 50f, 30f),
                s_seedMaterial, Color.White, 0f);
            Graphics.FillRectangle(new Rectangle(200f, 40f, 50f, 30f),
                s_seedMaterial, Color.White, 0f);

            batch.End();

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

            GL.BindImageTexture(0, s_voronoiInput.Handle, 0, false, 0,
                TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            s_distanceFieldComputer.Compute(320, 180, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
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
