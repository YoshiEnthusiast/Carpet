using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Carpet
{
    public class RayMarcher : System
    {
        private const int WorkGroupSize = 8;

        private readonly RenderPass _occlusionPass;
        private readonly RenderPass _intensityPass;
        private readonly RenderPass _rayMarchingPass;
        private readonly Pass _distanceFieldPass;

        private readonly JumpFloodComputer _jumpFloodComputer = new();
        private readonly DistanceFieldComputer _distanceFieldComputer = new();
        private readonly JumpFloodSeedComputer  _jumpFloodSeedComputer = new();
        
        private readonly RayMarchingMaterial _rayMarchingMaterial = new();

        private readonly List<RayEmitter> _emittersBuffer = [];
        private readonly List<RayOccluder> _occludersBuffer = [];

        private Texture2D _jumpFloodInput;
        private Texture2D _jumpFloodOutput;

        private Texture2D _distanceField;

        public RayMarcher(Scene scene, RenderPass occlusionPass, RenderPass intensityPass,
                Pass distanceFieldPass, RenderPass rayMarchingPass) : base(scene)
        {
            _occlusionPass = occlusionPass;
            _intensityPass = intensityPass;
            _distanceFieldPass = distanceFieldPass;
            _rayMarchingPass = rayMarchingPass;
        }

        public float Time { get; set; }
        public Texture2D Displacement { get; set; }

        public int RaysPerPixel { get; set; } = 20;
        public int MaxSteps { get; set; } = 100;
        public float SurfaceDistance { get; set; } = 0.01f;
        public float MaxDistance { get; set; } = 500f;

        public override void Initialize()
        {
            _occlusionPass.Render += RenderOcclusion;
            _intensityPass.Render += RenderIntensity;
            _distanceFieldPass.Render += ComputeDistanceField;
            _rayMarchingPass.Render += RayMarch;

            int width = _occlusionPass.Width;
            int height = _occlusionPass.Height;

            _jumpFloodInput = Texture2D.CreateEmpty(width, height);
            _jumpFloodOutput = Texture2D.CreateEmpty(width, height);

            _rayMarchingMaterial.Textures[0] = _occlusionPass.GetTexture();
            _rayMarchingMaterial.Textures[1] = _intensityPass.GetTexture();
        }

        public override void Terminate()
        {
            _occlusionPass.Render -= RenderOcclusion;
            _intensityPass.Render -= RenderIntensity;
            _distanceFieldPass.Render -= ComputeDistanceField;
            _rayMarchingPass.Render -= RayMarch;

            _jumpFloodInput.Bind();
            _jumpFloodInput.Delete();
            _jumpFloodOutput.Bind();
            _jumpFloodOutput.Delete();
        }

        private void RenderOcclusion()
        {
            foreach (RayOccluder occluder in Scene.GetComponentsOfType<RayOccluder>(_occludersBuffer))
                if (occluder.Visible)
                    occluder.DrawOcclusion();

            foreach (RayEmitter emitter in Scene.GetComponentsOfType<RayEmitter>(_emittersBuffer))
                if (emitter.Visible)
                    emitter.DrawEmission();
        }

        private void ComputeDistanceField()
        {
            int width = Maths.Ceiling((float)_occlusionPass.Width / WorkGroupSize);
            int height = Maths.Ceiling((float)_occlusionPass.Height / WorkGroupSize);

            _occlusionPass.GetTexture().
                BindImage(0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            _jumpFloodOutput.BindImage(1, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            _jumpFloodSeedComputer.Compute(width, height, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            int maxSide = Math.Max(_jumpFloodInput.Width, _jumpFloodInput.Height);
            int rounds = (int)Math.Log2(maxSide);

            for (int i = 0; i < rounds; i++)
            {
                int stepLength = maxSide / (int)Math.Pow(2, i + 1);

                Texture2D temp = _jumpFloodInput;
                _jumpFloodInput = _jumpFloodOutput;
                _jumpFloodOutput = temp;

                _jumpFloodInput.BindImage(0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
                _jumpFloodOutput.BindImage(1, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                _jumpFloodComputer.StepLength = stepLength;
                _jumpFloodComputer.Compute(width, height, 1);

                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            }

            _distanceField = _jumpFloodOutput;

            _distanceField.BindImage(0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);

            _distanceFieldComputer.Compute(width, height, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        private void RenderIntensity()
        {
            foreach (RayEmitter emitter in Scene.GetComponentsOfType<RayEmitter>(_emittersBuffer))
                if (emitter.Visible)
                    emitter.DrawIntensity();
        }


        private void RayMarch()
        {
            _rayMarchingMaterial.Textures[2] = Displacement;
            _rayMarchingMaterial.Displacement = Displacement is not null;
            _rayMarchingMaterial.Time = Time;

            _rayMarchingMaterial.RaysPerPixel = RaysPerPixel;
            _rayMarchingMaterial.MaxSteps = MaxSteps;
            _rayMarchingMaterial.SurfaceDistance = SurfaceDistance;
            _rayMarchingMaterial.MaxDistance = MaxDistance;

            Graphics.Draw(_distanceField, _rayMarchingMaterial, 
                    new Rectangle(0, 0, _distanceField.Width, _distanceField.Height), Color.White, 0f);
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
