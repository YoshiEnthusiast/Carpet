using System;
using System.Collections.Generic;
using System.Linq;

namespace Carpet
{
    public class Sprite : Component
    {
        private readonly Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        private Animation? _currentAnimation;
        private Rectangle _textureBounds;
        private readonly int _framesPerRow;

        private int _frame;
        private int _frameIndex;
        private float _timePassed;

        private float _delayMultiplier = 1f;

        public Sprite(VirtualTexture texture, int frameWidth, int frameHeight)
        {
            Texture = texture;

            int textureWidth = Texture.Width;  
            int textureHeight = Texture.Height;

            FrameWidth = Maths.Min(frameWidth, textureWidth);
            FrameHeight = Maths.Min(frameHeight, textureHeight);

            _framesPerRow = textureWidth / FrameWidth;

            SetCenterOrigin();
            UpdateTextureBounds();
        }

        public Sprite(string fileName, int frameWidth, int frameHeight) : this(Content.GetVirtualTexture(fileName), frameWidth, frameHeight)
        {

        }

        public Sprite(VirtualTexture texture) : this(texture, texture.Width, texture.Height)
        {

        }

        public Sprite(string fileName) : this(Content.GetVirtualTexture(fileName))
        {
            
        }

        private Sprite(VirtualTexture texture, int frameWidth, int frameHeight, int framesPerRow, Dictionary<string, Animation> animations, Rectangle bounds)
        {
            Texture = texture;

            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            _framesPerRow = framesPerRow;

            _animations = animations;
            _textureBounds = bounds;
        }

        public int Frame
        {
            get
            {
                return _frame;
            }

            set
            {
                _frame = value;
                _timePassed = 0;

                UpdateTextureBounds();
            }
        }

        public float DelayMultiplier
        {
            get
            {
                return _delayMultiplier;
            }

            set
            {
                if (value <= 0f)
                {
                    _delayMultiplier = 1f;

                    return;
                }

                _delayMultiplier = value;
            } 
        }

        public VirtualTexture Texture { get; private init; }
        public int FrameWidth { get; private init; }
        public int FrameHeight { get; private init; }

        public Material Material { get; set; }

        public string CurrentAnimationName { get; private set; }
        public bool AnimationFinished { get; private set; }

        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 Origin { get; set; }

        public Color Color { get; set; } = Color.White;
        public float Angle { get; set; }
        public float Depth { get; set; }

        public SpriteEffect VerticalEffect { get; set; }
        public SpriteEffect HorizontalEffect { get; set; }

        public float Width => FrameWidth * Scale.X;
        public float Height => FrameHeight * Scale.Y;
        public Vector2 Size => new Vector2(Width, Height);

        protected override void Update(float deltaTime)
        {
            if (_currentAnimation is null || AnimationFinished)
                return;

            _timePassed += deltaTime;

            while (true)
            {
                Animation animation = _currentAnimation.Value;

                AnimationSegment segment = animation.SegmentAt(_frameIndex);
                float delay = segment.Delay * DelayMultiplier;

                if (_timePassed < delay)
                    break;

                _timePassed -= delay;

                if (_frameIndex >= animation.SegmentsCount - 1)
                {
                    if (animation.Looped)
                        _frameIndex = 0;
                    else
                        AnimationFinished = true;
                }
                else
                {
                    _frameIndex++;
                }

                int frame = animation.SegmentAt(_frameIndex).Frame;
                Frame = frame;
            }
        }

        public void ResetAnimation()
        {
            if (_currentAnimation is null)
                return;

            Animation animation = _currentAnimation.Value;

            int frame = animation.SegmentAt(0).Frame;
            Frame = frame;

            AnimationFinished = false;
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
                CurrentAnimationName = null;

                return true;
            }
            else if (name == CurrentAnimationName)
            {
                return true;
            }

            if (!_animations.TryGetValue(name, out Animation animation))
                return false;

            _currentAnimation = animation;
            CurrentAnimationName = name;

            ResetAnimation();

            return true;
        }

        public void SetCenterOrigin()
        {
            Origin = new Vector2(FrameWidth, FrameHeight) / 2f;  
        }

        public Sprite Clone()
        {
            var sprite = new Sprite(Texture, FrameWidth, FrameHeight, _framesPerRow, new Dictionary<string, Animation>(_animations), _textureBounds);

            sprite.SetAnimation(CurrentAnimationName);

            return sprite;
        }

        protected override void Draw()
        {
            Draw(Position);
        }

        public void DrawOnDefaultPosition()
        {
            Draw();
        }

        public void Draw(Vector2 position)
        {
            Graphics.Draw(Texture, Material, _textureBounds, position, Scale, Origin, Color, Angle, HorizontalEffect, VerticalEffect, Depth);
        }

        public void Draw(float x, float y)
        {
            Draw(new Vector2(x, y));
        }

        private void UpdateTextureBounds()
        {
            float x = _frame % _framesPerRow * FrameWidth;
            float y = _frame / _framesPerRow * FrameHeight;

            _textureBounds = new Rectangle(x, y, FrameWidth, FrameHeight);
        }
    }
}
