using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public class Camera
    {
        private Matrix4 _viewMatrix;
        private bool _updateMatrix = true;

        private Vector2 _position;
        private Vector2 _origin;
        private Vector2 _scale = Vector2.One;
        private float _angle;

        public Camera(int maxWidth, int naxHeight)
        {
            MaxWidth = maxWidth;
            MaxHeight = naxHeight;
        }

        public int MaxWidth { get; private init; }
        public int MaxHeight { get; private init; }

        public float Zoom { get; set; } = 1f;

        public Vector2 Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
                _updateMatrix = true;
            }
        }

        public Vector2 Origin
        {
            get
            {
                return _origin;
            }

            set
            {
                _origin = value;
                _updateMatrix = true;
            }
        }

        public Vector2 Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                _scale = value;
                _updateMatrix = true;
            }
        }

        public float Angle
        {
            get
            {
                return _angle;
            }

            set
            {
                _angle = value;
                _updateMatrix = true;
            }
        }

        public float Width
        {
            get
            {
                return MaxWidth * _scale.X;
            }

            set
            {
                _scale = new Vector2(value / MaxWidth, _scale.Y);
            }
        }

        public float Heigth
        {
            get
            {
                return MaxHeight * _scale.Y;
            }

            set
            {
                _scale = new Vector2(_scale.X, value / MaxHeight);
            }
        }

        public float X
        {
            get
            {
                return _position.X;
            }

            set
            {
                Position = new Vector2(value, _position.Y);
            }
        }

        public float Y
        {
            get
            {
                return _position.Y;
            }

            set
            {
                Position = new Vector2(_position.X, value);
            }
        }

        public virtual void Update()
        {

        }

        public Matrix4 GetViewMatrix()
        {
            if (_updateMatrix)
            {
                float originX = _origin.RoundedX;
                float originY = _origin.RoundedY;

                _viewMatrix = Matrix4.CreateScale(_scale.X, _scale.Y, 0f);

                if (_angle != 0f)
                    _viewMatrix *= Matrix4.CreateTranslation(-originX, -originY, 0f) * Matrix4.CreateRotationZ(_angle) * Matrix4.CreateTranslation(originX, originY, 0f);

                _viewMatrix *= Matrix4.CreateTranslation(_position.RoundedX - originX, _position.RoundedY - originY, 0f);

                _updateMatrix = false;
            }

            return _viewMatrix;
        }

        public void SetCenterOrigin()
        {
            Origin = new Vector2(MaxWidth, MaxHeight) / 2f;
        }
    }
}
