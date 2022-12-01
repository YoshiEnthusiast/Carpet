using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public class Camera
    {
        private readonly int _maxWidth;
        private readonly int _maxHeight;

        private Matrix4 _viewMatrix;
        private bool _updateMatrix = true;

        private Vector2 _position;
        private Vector2 _origin;
        private Vector2 _scale = Vector2.One;
        private float _angle;

        public Camera(int maxWidth, int naxHeight)
        {
            _maxWidth = maxWidth;
            _maxHeight = naxHeight;
        }

        public int MaxWidth => _maxWidth;
        public int MaxHeight => _maxHeight;

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
                return _maxWidth * _scale.X;
            }

            set
            {
                _scale = new Vector2(value / _maxWidth, _scale.Y);
            }
        }

        public float Heigth
        {
            get
            {
                return _maxHeight * _scale.Y;
            }

            set
            {
                _scale = new Vector2(_scale.X, value / _maxHeight);
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
                _viewMatrix = Utilities.CreateTransformMatrix(_position, _origin, _scale, _angle);

                _updateMatrix = false;
            }

            return _viewMatrix;
        }

        public void SetCenterOrigin()
        {
            Origin = new Vector2(_maxWidth, _maxHeight) / 2f;
        }
    }
}
