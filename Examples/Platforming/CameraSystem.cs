using System;

namespace Carpet.Platforming
{
    public class CameraSystem : System
    {
        private readonly float _lerpAmount;
        private bool _forcePosition = true;

        public CameraSystem(float lerpAmount, Scene scene) : base(scene)
        {
            _lerpAmount = lerpAmount;
        }

        public Vector2 CameraPosition { get; private set; }

        public override void Update(float deltaTime)
        {
            Player player = Scene.GetEntityOfType<Player>();    

            if (player is not null)
            {
                Camera camera = Game.ForegroundLayer.Camera;
                Vector2 playerPosition = player.Position;

                if (_forcePosition)
                {
                    camera.Position = playerPosition;
                    _forcePosition = false;

                    return;
                }

                Vector2 oldPosition = camera.Position;
                Vector2 lerpedPosition = Vector2.Lerp(oldPosition, playerPosition, _lerpAmount);

                CameraPosition = lerpedPosition;
                camera.Position = CameraPosition;
            }
            else
            {
                _forcePosition = true;
            }
        }
    }
}
