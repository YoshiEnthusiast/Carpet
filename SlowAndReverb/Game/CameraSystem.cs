using System;

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

        public Vector2 CameraPosition { get; private set; }

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
                Vector2 lerpedPosition = Vector2.Lerp(oldPosition, playerPosition, 0.07f);

                if (Input.IsPressed(Key.Up))
                {
                    camera.Zoom += 0.05f;
                    Console.WriteLine(camera.Zoom);
                }
                else if (Input.IsPressed(Key.Down))
                {
                    camera.Zoom -= 0.05f;
                    Console.WriteLine(camera.Zoom);
                }

                CameraPosition = playerPosition;
                camera.Position = CameraPosition;
            }
            else
            {
                _forcePosition = true;
            }
        }
    }
}
