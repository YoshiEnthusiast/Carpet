using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class Sprite
    {
        private readonly Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        private readonly Texture _texture;

        private readonly int _frameWidth;
        private readonly int _frameHeight;
        private readonly int _framesPerRow;

        private Animation? _currentAnimation;
        private string _currentAnimationName;

        private Rectangle _textureBounds;

        private int _frame;
        private int _frameIndex;
        private int _timePassed;

        private bool _animationFinished;

        public Sprite(Texture texture, int frameWidth, int frameHeight)
        {
            _texture = texture;

            int textureWidth = _texture.Width;  
            int textureHeight = _texture.Height;

            _frameWidth = Math.Min(frameWidth, textureWidth);
            _frameHeight = Math.Min(frameHeight, textureHeight);

            _framesPerRow = textureWidth / _frameWidth;

            UpdateTextureBounds();
        }

        public Sprite(string fileName, int frameWidth, int frameHeight) : this(Content.GetTexture(fileName), frameWidth, frameHeight)
        {

        }

        public Sprite(Texture texture) : this(texture, texture.Width, texture.Height)
        {

        }

        public Sprite(string fileName) : this(Content.GetTexture(fileName))
        {

        }

        private Sprite(Texture texture, int frameWidth, int frameHeight, int framesPerRow, Dictionary<string, Animation> animations, Rectangle bounds)
        {
            _texture = texture;

            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _framesPerRow = framesPerRow;

            _animations = animations;
            _textureBounds = bounds;
        }

        public Material Material { get; set; }

        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 Origin { get; set; }

        public Color Color { get; set; } = Color.White;
        public float Angle { get; set; }
        public float Depth { get; set; }

        public SpriteEffect VerticalEffect { get; set; }
        public SpriteEffect HorizontalEffect { get; set; }

        public Texture Texture => _texture;
        public int Frame => _frame;
        public float Width => _frameWidth * Scale.X;
        public float Height => _frameHeight * Scale.Y;
        public float FrameWidth => _frameWidth;
        public float FrameHeight => _frameHeight;

        public string CurrentAnimationName => _currentAnimationName;
        public bool AnimationFinished => _animationFinished;

        public void UpdateFrame()
        {
            if (_currentAnimation is null || _animationFinished)
                return;

            Animation animation = _currentAnimation.Value;

            IEnumerable<AnimationSegment> segments = animation.Segments;
            AnimationSegment segment = segments.ElementAt(_frameIndex);

            _timePassed++;

            if (_timePassed >= segment.Delay)
            {
                if (_frameIndex >= segments.Count() - 1)
                {
                    if (animation.Looped)
                        _frameIndex = 0;
                    else
                        _animationFinished = true;
                }
                else
                {
                    _frameIndex++;
                }

                SetFrame(segments.ElementAt(_frameIndex).Frame);
            }
        }

        public void SetFrame(int frame)
        {
            _frame = frame;
            _timePassed = 0;

            UpdateTextureBounds();
        }

        public void ResetAnimation()
        {
            if (_currentAnimation is null)
                return;

            Animation animation = _currentAnimation.Value;
            SetFrame(animation.Segments.First().Frame);

            _animationFinished = false;
        }

        public void AddAnimation(string name, Animation animation)
        {
            if (_animations.ContainsKey(name))
            {
                _animations[name] = animation;

                return;
            }

            _animations.Add(name, animation);
        }

        public void AddAnimation(string name, bool loop, IEnumerable<AnimationSegment> segments)
        {
            AddAnimation(name, new Animation(loop, segments));
        }

        public void AddAnimation(string name, bool loop, int delay, IEnumerable<int> frames)
        {
            AddAnimation(name, new Animation(delay, loop, frames));
        }

        public bool SetAnimation(string name)
        {
            if (name is null)
            {
                _currentAnimation = null;
                _currentAnimationName = null;

                return true;
            }

            if (!_animations.TryGetValue(name, out Animation animation))
                return false;

            _currentAnimation = animation;
            _currentAnimationName = name;

            ResetAnimation();

            return true;
        }

        public void SetCenterOrigin()
        {
            Origin = new Vector2(_frameWidth, _frameHeight) / 2f;  
        }

        public Sprite Clone()
        {
            var sprite = new Sprite(_texture, _frameWidth, _frameHeight, _framesPerRow, new Dictionary<string, Animation>(_animations), _textureBounds);

            sprite.SetAnimation(_currentAnimationName);

            return sprite;
        }

        public void Draw(Vector2 position)
        {
            DrawWithoutUpdate(position);

            UpdateFrame();
        }

        public void Draw(float x, float y)
        {
            Draw(new Vector2(x, y));
        }

        public void DrawWithoutUpdate(Vector2 position)
        {
            Graphics.Draw(_texture, Material, _textureBounds, position, Scale, Origin, Color, Angle, HorizontalEffect, VerticalEffect, Depth);
        }

        public void DrawWithoutUpdate(float x, float y)
        {
            DrawWithoutUpdate(new Vector2(x, y));
        }

        private void UpdateTextureBounds()
        {
            float x = _frame % _framesPerRow * _frameWidth;
            float y = _frame / _framesPerRow * _frameHeight;

            _textureBounds = new Rectangle(x, y, _frameWidth, _frameHeight);
        }
    }
}
