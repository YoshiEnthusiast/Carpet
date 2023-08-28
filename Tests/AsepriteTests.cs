using StbImageSharp;
using Xunit.Abstractions;
using AsepriteLayer = SlowAndReverb.Aseprite.Layer;
using Cel = SlowAndReverb.Aseprite.Cel;
using LoopAnimationDirection = SlowAndReverb.Aseprite.LoopAnimationDirection;
using Slice = SlowAndReverb.Aseprite.Slice;
using SliceKey = SlowAndReverb.Aseprite.SliceKey;
using Tag = SlowAndReverb.Aseprite.Tag;
using UserData = SlowAndReverb.Aseprite.UserData;
using UserDataChunk = SlowAndReverb.Aseprite.UserDataChunk;

namespace SlowAndReverb.Tests
{
    public class AsepriteTests : IClassFixture<ImageData>
    {
        private const string AsepriteExtension = ".ase";

        private readonly ImageData _imageData;
        private readonly ITestOutputHelper _output;

        public AsepriteTests(ImageData imageData, ITestOutputHelper output)
        {
            _imageData = imageData;
            _output = output;
        }

        [Theory]
        [InlineData("vase")]
        [InlineData("lantern")]
        [InlineData("transparency")]
        [InlineData("transparent2")]
        [InlineData("grayscale")]
        [InlineData("grayscale_transparent")]
        [InlineData("indexed")]
        public void FromFile_RenderSprite(string fileName)
        {
            Aseprite aseprite = LoadAseprite(fileName);
            byte[] asepriteData = aseprite.Frames[0].RenderedImage;

            Assert.Equal(asepriteData, _imageData.Get(fileName));
        }

        [Theory]
        [InlineData("vase", 1, 2, 10, 10, true, 5, 6)]
        [InlineData("lantern", 3, 7, 2, 3, false, 0, 0)]
        public void FromFile_ParseSlices(string fileName, int x, int y, int width, int height, 
            bool hasPivot, int pivotX, int pivotY)
        {
            Aseprite aseprite = LoadAseprite(fileName);

            SliceKey key = aseprite.Slices[0].Keys[0];

            var expextedKey = new SliceKey()
            {
                FrameNumber = 0,
                OriginX = x,
                OriginY = y,
                Width = width,
                Height = height,
                HasPivot = hasPivot,
                PivotX = pivotX,
                PivotY = pivotY
            };

            Assert.Equivalent(expextedKey, key);
        }

        [Theory]
        [InlineData("vase", 0, "Tag1", 0, 3, LoopAnimationDirection.Forward, 0, 106, 205, 91)]
        [InlineData("vase", 1, "Tag2", 2, 6, LoopAnimationDirection.PingPong, 0, 243, 206, 82)]
        [InlineData("vase", 2, "Tag3", 4, 8, LoopAnimationDirection.Reverse, 0, 209, 134, 223)]
        public void FromFile_ParseTags(string fileName, int tagIndex, string name, int fromFrame, int toFrame, 
            LoopAnimationDirection animationDirection, int repeat, byte r, byte g, byte b)
        {
            Aseprite aseprite = LoadAseprite(fileName);
            Tag tag = aseprite.Tags[tagIndex];

            var expextedTag = new Tag()
            {
                Name = name,
                FromFrame = fromFrame,
                ToFrame = toFrame,
                LoopAnimationDirection = animationDirection,
                Repeat = repeat,
                Color = new Color(r, g, b)
            };

            Assert.Equivalent(expextedTag, tag);
        }

        [Theory]
        [InlineData("vase", "Layer user data", 167, 63, 188)]
        public void FromFile_ParseLayerUserData(string fileName, string text, byte r, byte g, byte b)
        {
            Aseprite aseprite = LoadAseprite(fileName);
            AsepriteLayer layer = aseprite.Layers[0];
            
            CheckUserData(layer, text, r, g, b);
        }

        [Theory]
        [InlineData("vase", "Cel user data", 194, 255, 64)]
        public void FromFile_ParseCelUserData(string fileName, string text, byte r, byte g, byte b)
        {
            Aseprite aseprite = LoadAseprite(fileName);
            Cel cel = aseprite.Frames[0].Cels[0];

            CheckUserData(cel, text, r, g, b);
        }

        [Theory]
        [InlineData("vase", "Slice user data", 63, 201, 71)]
        public void FromFile_ParseSliceUserData(string fileName, string text, byte r, byte g, byte b)
        {
            Aseprite aseprite = LoadAseprite(fileName);
            Slice slice = aseprite.Slices[0];

            CheckUserData(slice, text, r, g, b);
        }

        private void CheckUserData(UserDataChunk chunk, string text, byte r, byte g, byte b)
        {
            var expectedData = new UserData()
            {
                Text = text,
                Color = new Color(r, g, b)
            };

            Assert.Equivalent(expectedData, chunk.UserData);
        } 

        private Aseprite LoadAseprite(string fileName)
        {
            fileName = Path.Combine(Tests.ContentFolder, fileName);
            fileName = Path.ChangeExtension(fileName, AsepriteExtension);

            return Aseprite.FromFile(fileName);
        }
    }

    public sealed class ImageData : IDisposable
    {
        private const string PngExtension = ".png";

        private readonly Dictionary<string, byte[]> _data = new Dictionary<string, byte[]>();

        public ImageData()
        {
            StbImage.stbi_set_flip_vertically_on_load(1);

            foreach (string path in Directory.EnumerateFiles(Tests.ContentFolder))
            {
                if (Path.GetExtension(path) != PngExtension)
                    continue;

                byte[] data = Load(path);
                string fileName = Path.GetFileName(path);
                fileName = Path.ChangeExtension(fileName, null);

                _data.Add(fileName, data);
            }
        }

        public byte[] Get(string fileName)
        {
            return _data[fileName];
        }

        public void Dispose()
        {
            StbImage.stbi_set_flip_vertically_on_load(0);
        }

        private byte[] Load(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream);

                return image.Data;
            }
        }
    }
}