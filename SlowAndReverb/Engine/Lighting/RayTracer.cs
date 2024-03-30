using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public class RayTracer
    {
        private static readonly JumpFloodSeedMaterial s_seedMaterial = new();
        private static readonly JumpFloodComputer s_computer = new();

        public static void Test()
        {
            Texture2D voronoiInput = Texture2D.CreateEmpty(320, 180);
            Texture2D voronoiOutput = Texture2D.CreateEmpty(320, 180);

            SpriteBatch batch = Graphics.SpriteBatch;
            RenderTarget target = RenderTarget.FromTexture(voronoiOutput);

            batch.Begin(target, BlendMode.Opaque, Color.Transparent, null);

            s_seedMaterial.Resolution = new Vector2(320f, 180f);

            Graphics.FillRectangle(new Rectangle(10f, 10f, 40f, 40f),
                s_seedMaterial, Color.White, 0f);
            Graphics.FillRectangle(new Rectangle(100f, 80f, 30f, 20f), 
                s_seedMaterial, Color.White, 0f);

            batch.End();

            int maxSide = Maths.Max(voronoiInput.Width, voronoiInput.Height);
            int steps =(int)Math.Log2(maxSide);

            for (int i = 0; i < steps; i++)
            {
                int stepLength = maxSide / (int)Math.Pow(2, i + 1);

                Texture2D temp = voronoiInput;
                voronoiInput = voronoiOutput;
                voronoiOutput = temp;

                GL.BindImageTexture(0, voronoiInput.Handle, 0, false, 0,
                    TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

                GL.BindImageTexture(1, voronoiOutput.Handle, 0, false, 0,
                    TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                s_computer.StepLength = stepLength;
                s_computer.Compute(320, 180, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            }

            batch.Begin(RenderTarget.FromTexture(voronoiInput), BlendMode.Opaque, Color.Black, null);
            Graphics.Draw(voronoiOutput, null, new Rectangle(), Color.White, 0f);
            batch.End();
        }

        private class JumpFloodComputer : Computer
        {
            public JumpFloodComputer()
            {
                ShaderProgram = Content.GetComputeShaderProgram("jumpFlood");
            }

            [Uniform("u_StepLength")] public int StepLength { get; set; }
        }
    }
}
