﻿using System;

namespace SlowAndReverb
{
    public class CameraSystem : System
    {
        private readonly float _lerpMultiplier;
        private bool _forcePosition = true;

        public CameraSystem(float lerpMultiplier, Scene scene) : base(scene)
        {
            _lerpMultiplier = lerpMultiplier;
        }

        public override void Update(float deltaTime)
        {
            Player player = Scene.GetEntityOfType<Player>();    

            if (player is not null)
            {
                Camera camera = Layers.Foreground.Camera;
                Vector2 playerPosition = player.Position;

                if (_forcePosition)
                {
                    camera.Position = playerPosition;
                    _forcePosition = false;

                    return;
                }

                Vector2 oldPosition = camera.Position;
                Vector2 lerpedPosition = Vector2.Lerp(oldPosition, playerPosition, 1f - (float)Math.Pow(0.01f, deltaTime / 30f));

                camera.Position = playerPosition;
            }
            else
            {
                _forcePosition = true;
            }
        }
    }
}