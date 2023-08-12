using OpenTK.Graphics.ES11;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SlowAndReverb
{
    // TODO: Other color depths

    public sealed class Aseprite
    {
        private const ushort FormatIdentifier = 0xA5E0;

        private const ushort LayerChunkID = 0x2004;
        private const ushort CelChunkID = 0x2005;
        private const ushort CelExtraChunkID = 0x2006;
        private const ushort ColorProfileChunkID = 0x2007;
        private const ushort ExternalFilesChunkID = 0x2008;
        private const ushort TagsChunkID = 0x2018;
        private const ushort PaletteChunkID = 0x2019;
        private const ushort UserDataChunkID = 0x2020;
        private const ushort SliceChunkID = 0x2022;
        private const ushort TilesetChunkID = 0x2023;

        private const ushort OldPaletteChunk1ID = 0x0004;
        private const ushort OldPaletteChunk2ID = 0x0011;
        private const ushort MaskChunkID = 0x2016;
        private const ushort PathChunkID = 0x2017;

        private Aseprite(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                uint fileSize = reader.ReadUInt32();

                ushort magicNumber = reader.ReadUInt16();

                ushort framesCount = reader.ReadUInt16();

                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();

                Mode = (ColorMode)reader.ReadUInt16();

                if (Mode != ColorMode.RGBA)
                    throw new NotImplementedException("The only implemented color mode is ColorMode.RGBA");

                uint flags = reader.ReadUInt32();

                ushort deprecatedFrameSpeed = reader.ReadUInt16();

                uint a = reader.ReadUInt32();
                uint b = reader.ReadUInt32();

                byte transparentColorIndex = reader.ReadByte();

                Move(reader, 3);

                ushort paletteEntriesCount = reader.ReadUInt16();

                Palette = new PaletteEntry[paletteEntriesCount];

                byte pixelWidth = reader.ReadByte();
                byte pixelHeight = reader.ReadByte();

                short gridX = reader.ReadInt16();
                short gridY = reader.ReadInt16();

                ushort gridWidth = reader.ReadUInt16();
                ushort gridHeight = reader.ReadUInt16();

                Move(reader, 84);

                Frames = new Frame[framesCount];

                var layers = new List<Layer>();
                var slices = new List<Slice>();

                for (int i = 0; i < framesCount; i++)
                {
                    uint frameSize = reader.ReadUInt32();

                    ushort layerMagicNumber = reader.ReadUInt16();

                    ushort oldChunkCount = reader.ReadUInt16();
                    ushort frameDuration = reader.ReadUInt16();

                    Move(reader, 2);

                    uint chunkCount = reader.ReadUInt32();

                    if (chunkCount == 0)
                        chunkCount = oldChunkCount;

                    var image = new byte[Width * Height * Color.ComponentsCount];

                    var cels = new List<Cel>();

                    for (int j = 0; j < chunkCount; j++)
                    {
                        uint chunkSize = reader.ReadUInt32();
                        ushort chunkType = reader.ReadUInt16();

                        int maxPosition = (int)stream.Position + (int)chunkSize - 6;

                        switch (chunkType)
                        {
                            case LayerChunkID:
                                Layer layer = ParseLayer(reader);
                                layers.Add(layer);

                                break;

                            case CelChunkID:
                                Cel cel = ParseCel(reader, maxPosition);
                                cels.Add(cel);
                                RenderCel(cel, image, layers);

                                break;

                            case PaletteChunkID:
                                ReadPalette(reader);
                                
                                break;

                            case ColorProfileChunkID:
                                Profile = ParseColorProfile(reader);

                                break;

                            case SliceChunkID:
                                var slice = ParseSlice(reader);
                                slices.Add(slice);

                                break;

                            case TagsChunkID:
                                Tags = ParseTags(reader);

                                break;
                        }

                        stream.Position = maxPosition;
                    }

                    var frame = new Frame()
                    {
                        Cels = cels.ToArray(),
                        RenderedImage = image,
                        Duration = frameDuration
                    };

                    Frames[i] = frame;
                }

                Layers = layers.ToArray();
                Slices = slices.ToArray();
            }
        }

        public Frame[] Frames { get; private init; }
        public Layer[] Layers { get; private init; }
        public Slice[] Slices { get; private init; }
        public Tag[] Tags { get; private init; }
        public PaletteEntry[] Palette { get; private init; }

        public ColorMode Mode { get; private init; }
        public ColorProfile Profile { get; private init; }

        public int Width { get; private init; }
        public int Height { get; private init; }


        public static Aseprite FromStream(Stream stream)
        {
            return new Aseprite(stream);
        }

        public static Aseprite FromFile(string path)
        {
            using (FileStream stream = File.OpenRead(path))
                return FromStream(stream);
        }

#region Parsing

        private Layer ParseLayer(BinaryReader reader)
        {
            LayerFlag layerFlag = (LayerFlag)reader.ReadUInt16();
            LayerType layerType = (LayerType)reader.ReadUInt16();

            ushort layerChildLevel = reader.ReadUInt16();

            ushort defaultLayerWidth = reader.ReadUInt16();
            ushort defaultLayerHeight = reader.ReadUInt16();

            LayerBlendMode layerBlendMode = (LayerBlendMode)reader.ReadUInt16();

            byte layerOpacity = reader.ReadByte();

            Move(reader, 3);

            string layerName = ReadString(reader);

            int? tilesetIndex = null;

            if (layerType == LayerType.Tilemap)
                tilesetIndex = (int)reader.ReadUInt32();

            var layer = new Layer()
            {
                Flag = layerFlag,
                Type = layerType,
                ChildLevel = layerChildLevel,
                BlendMode = layerBlendMode,
                Opacity = layerOpacity,
                Name = layerName,
                TilesetIndex = tilesetIndex
            };

            return layer;
        }

        private Cel ParseCel(BinaryReader reader, int maxPosition)
        {
            ushort layerIndex = reader.ReadUInt16();

            short x = reader.ReadInt16();
            short y = reader.ReadInt16();

            byte opacity = reader.ReadByte();

            CelType type = (CelType)reader.ReadUInt16();

            short zIndex = reader.ReadInt16();

            Move(reader, 5);

            ushort width = 0;
            ushort height = 0;

            int? linkedFramePosition = null;
            TilemapData? tilemapData = null;

            if (type == CelType.RawImageData || type == CelType.CompressedImage)
            {
                width = reader.ReadUInt16();
                height =  reader.ReadUInt16();
            }
            else if (type == CelType.LinkedCel)
            {
                linkedFramePosition = reader.ReadUInt16();
            }
            else if (type == CelType.CompressedTilemap)
            {
                ushort tileWidth = reader.ReadUInt16();
                ushort tileHeight = reader.ReadUInt16();

                ushort bitsPerTile = reader.ReadUInt16();

                uint tileIDBitmask = reader.ReadUInt32();
                uint xFlipMask = reader.ReadUInt32();
                uint yFlipMask = reader.ReadUInt32();
                uint rotation90Mask = reader.ReadUInt32();

                Move(reader, 10);

                tilemapData = new TilemapData()
                {
                    TileWidth = tileWidth,
                    TileHeight = tileHeight,
                    BitsPerTile = bitsPerTile,
                    TileIDBitmask = tileIDBitmask,
                    XFlipBitmask = xFlipMask,
                    YFlipBitmask = yFlipMask,
                    Rotation90Mask = rotation90Mask,
                };
            }

            int position = (int)reader.BaseStream.Position;
            int length = maxPosition - position;
            byte[] data = reader.ReadBytes(length);

            var cel = new Cel()
            {
                Type = type,
                LayerIndex = layerIndex,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Opacity = opacity,
                ZIndex = zIndex,
                Data = data,
                LinkedFramePosition = linkedFramePosition,
                TilemapData = tilemapData
            };

            return cel;
        }

        private ColorProfile ParseColorProfile(BinaryReader reader)
        {
            ColorProfileType type = (ColorProfileType)reader.ReadUInt16();
            ushort flags = reader.ReadUInt16();

            byte[] gamma = reader.ReadBytes(4);

            Move(reader, 8);

            byte[] iccData = null;

            if (type == ColorProfileType.IICProifle)
            {
                uint length = reader.ReadUInt32();

                iccData = reader.ReadBytes((int)length);
            }

            var colorProfile = new ColorProfile()
            {
                Type = type,
                Gamma = gamma,
                ICCData = iccData
            };

            return colorProfile;
        }

        private Slice ParseSlice(BinaryReader reader)
        {
            uint keysCount = reader.ReadUInt32();
            uint flags = reader.ReadUInt32();

            Move(reader, 4);

            string name = ReadString(reader);

            var keys = new SliceKey[keysCount];

            for (int i = 0; i < keysCount; i++)
            {
                int frameNumber = (int)reader.ReadUInt32();

                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                int width = (int)reader.ReadUInt32();
                int height = (int)reader.ReadUInt32();

                bool hasPivot = false;
                int pivotX = 0;
                int pivotY = 0;

                if (CheckFlag(flags, 0x0001))
                    Move(reader, 16);

                if (CheckFlag(flags, 0x0002))
                {
                    hasPivot = true;
                    pivotX = reader.ReadInt32();
                    pivotY = reader.ReadInt32();
                }

                var key = new SliceKey()
                {
                    FrameNumber = frameNumber,
                    OriginX = x,
                    OriginY = y,
                    Width = width,
                    Height = height,
                    HasPivot = hasPivot,
                    PivotX = pivotX,
                    PivotY = pivotY
                };

                keys[i] = key;
            }

            var slice = new Slice()
            {
                Name = name,
                Keys = keys
            };

            return slice;
        }

        private Tag[] ParseTags(BinaryReader reader)
        {
            ushort tagsCount = reader.ReadUInt16();

            Move(reader, 8);

            var tags = new Tag[tagsCount];

            for (int i = 0; i < tagsCount; i++)
            {
                int fromFrame = (int)reader.ReadUInt16();
                int toFrame = (int)reader.ReadUInt16(); 

                LoopAnimationDirection animationDirection = (LoopAnimationDirection)reader.ReadByte();

                int repeat = (int)reader.ReadUInt16();

                Move(reader, 6);

                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();

                var color = new Color(r, g, b);

                reader.ReadByte();

                string name = ReadString(reader);

                var tag = new Tag()
                {
                    FromFrame = fromFrame,
                    ToFrame = toFrame,
                    Name = name,
                    LoopAnimationDirection = animationDirection,
                    Repeat = repeat,
                    Color = color
                };

                tags[i] = tag;
            }

            return tags;
        }

        private void ReadPalette(BinaryReader reader)
        {
            uint size = reader.ReadUInt32();

            int startIndex = (int)reader.ReadUInt32();
            int endIndex = (int)reader.ReadUInt32();

            Move(reader, 8);

            for (int i = startIndex; i <= endIndex; i++)
            {
                ushort entryFlags = reader.ReadUInt16();
                string name = null;

                byte red = reader.ReadByte();
                byte green = reader.ReadByte();
                byte blue = reader.ReadByte();
                byte alpha = reader.ReadByte();

                if (CheckFlag(entryFlags, 0x01))
                    name = ReadString(reader);

                var color = new Color(red, green, blue, alpha);
                var entry = new PaletteEntry(color, name);

                Palette[i] = entry;
            }
        }

        private string ReadString(BinaryReader reader)
        {
            ushort length = reader.ReadUInt16();
            byte[] buffer = reader.ReadBytes(length);

            string result = Encoding.UTF8.GetString(buffer, 0, length);

            return result;
        } 

#endregion

        private void RenderCel(Cel cel, byte[] buffer, List<Layer> layers)
        {
            if (cel.Type == CelType.LinkedCel)
            {
                int framePosition = cel.LinkedFramePosition.Value;
                Frame linkedFrame = Frames[framePosition];

                foreach (Cel other in linkedFrame.Cels)
                {
                    if (other.LayerIndex == cel.LayerIndex)
                    {
                        cel = other;
                        break;
                    }
                }   
            }

            int width = cel.Width;
            int height = cel.Height;

            CelType type = cel.Type;

            if (type == CelType.CompressedTilemap)
                return;

            int layerIndex = cel.LayerIndex;
            Layer layer = layers[layerIndex];

            if (layer.BlendMode != LayerBlendMode.Normal)
                throw new NotImplementedException("None of the Aseprite blend modes are implemented except for LayerBlendMode.Normal");

            byte[] celBuffer = cel.Data;

            if (type == CelType.CompressedImage)
            {
                sbyte[] inputBuffer = (sbyte[])(Array)celBuffer;
                
                unsafe
                {
                    fixed (sbyte* inputBufferPointer = inputBuffer)
                    {
                        int length = width * height * Color.ComponentsCount;
                        sbyte[] outputBuffer = new sbyte[length];

                        fixed (sbyte* outputBufferPointer = outputBuffer)
                        {
                            StbImage.stbi_zlib_decode_buffer(outputBufferPointer, length, 
                                inputBufferPointer, inputBuffer.Length);

                            celBuffer = (byte[])(Array)(outputBuffer);
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int bufferX = cel.X + x;
                    int bufferY = Height - cel.Y - y;

                    int celBufferIndex = GetBufferIndex(x, y, width);
                    int bufferIndex = GetBufferIndex(bufferX, bufferY, Width);

                    int sourceR = celBuffer[celBufferIndex];
                    int sourceG = celBuffer[celBufferIndex + 1];
                    int sourceB = celBuffer[celBufferIndex + 2];
                    int sourceA = celBuffer[celBufferIndex + 3];

                    int destinationR = buffer[bufferIndex];
                    int destinationG = buffer[bufferIndex + 1];
                    int destinationB = buffer[bufferIndex + 2];
                    int destinationA = buffer[bufferIndex + 3];

                    sourceA = MulUn8(sourceA, cel.Opacity);

                    int resultA = sourceA + destinationA - MulUn8(destinationA, sourceA);

                    if (resultA == 0)
                        continue;

                    int ratio = sourceA / resultA;

                    int resultR = destinationR + (sourceR - destinationR) * ratio;
                    int resultG = destinationG + (sourceG - destinationG) * ratio;
                    int resultB = destinationB + (sourceB - destinationB) * ratio;

                    buffer[bufferIndex] = (byte)resultR;
                    buffer[bufferIndex + 1] = (byte)resultG;
                    buffer[bufferIndex + 2] = (byte)resultB;
                    buffer[bufferIndex + 3] = (byte)resultA;
                }
            }
        }

        private int MulUn8(int a, int b)
        {
            int t = a * b + 0x80;

            return ((t >> 8) + t) >> 8;
        }

        private int GetBufferIndex(int x, int y, int width)
        {
            return (y * width + x) * Color.ComponentsCount;
        }

        private bool CheckFlag(uint flags, uint flag)
        {
            return (flags & flag) != 0;
        }

        private void Move(BinaryReader reader, int by)
        {
            reader.BaseStream.Position += by;
        }

#region Enums

        public enum ColorMode : ushort
        {
            Indexed = 8,

            Grayscale = 16,

            RGBA = 32
        }

        public enum LayerType : ushort
        {
            Normal,

            Group,

            Tilemap
        }

        [Flags]
        public enum LayerFlag : ushort
        {
            Visible = 1,

            Editable = 2,

            LockMovement = 4,

            Background = 8,

            PreferLinkedCels = 16,

            LayerGroupCollapsed = 32,

            ReferenceLayer = 64
        }

        public enum LayerBlendMode : ushort
        {
            Normal,

            Multiply,

            Screen,

            Overlay,

            Darken,

            Lighten,

            ColorDodge,

            ColorBurn,

            HardLight,

            SoftLight,

            Difference,

            Exclusion,

            Hue,

            Saturation,

            Color,

            Luminosity,

            Addition,

            Subtract,

            Divide
        }

        public enum CelType : ushort
        {
            RawImageData,

            LinkedCel,

            CompressedImage,

            CompressedTilemap
        }

        public enum ColorProfileType : ushort
        {
            NoColorProfile,

            SRGB,

            IICProifle
        }

        public enum LoopAnimationDirection : byte
        {
            Forward,

            Reverse,

            PingPong,

            PingPongReverse
        }

#endregion

#region ChunkTypes

        public sealed class Frame
        {
            public required Cel[] Cels { get; init; }

            public byte[] RenderedImage { get; init; }

            public required int Duration { get; init; }
        }

        public sealed class Layer
        {
            public required LayerFlag Flag { get; init; }

            public required LayerType Type { get; init; }

            public required int ChildLevel { get; init; }

            public required LayerBlendMode BlendMode { get; init; }

            public required byte Opacity { get; init; }

            public required string Name { get; init; }

            public int? TilesetIndex { get; init; }
        }

        public sealed class Cel
        {
            public required CelType Type { get; init; }

            public required int LayerIndex { get; init; }

            public required int X { get; init; }

            public required int Y { get; init; }

            public required int Width { get; init; }

            public required int Height { get; init; }

            public required byte Opacity { get; init; }

            public required int ZIndex { get; init; }

            public required byte[] Data { get; init; }

            public required int? LinkedFramePosition { get; init; }

            public required TilemapData? TilemapData { get; init; }
        }

        public sealed class ColorProfile
        {
            public required ColorProfileType Type { get; init; }

            public required byte[] Gamma { get; init; }

            public required byte[] ICCData { get; init; }
        }

        public sealed class Slice
        {
            public required string Name { get; init; }

            public required SliceKey[] Keys { get; init; }
        }

        public sealed class SliceKey
        {
            public required int FrameNumber { get; init; }

            public required int OriginX { get; init; }

            public required int OriginY { get; init; }

            public required int Width { get; init; }

            public required int Height { get; init; }

            public required bool HasPivot { get; init; }

            public required int PivotX { get; init; }
            
            public required int PivotY { get; init; }
        }

        public sealed class Tag
        {
            public required string Name { get; init; }

            public required int FromFrame { get; init; }

            public required int ToFrame { get; init; }

            public required LoopAnimationDirection LoopAnimationDirection { get; init; }

            public required int Repeat { get; init; }

            public required Color Color { get; init; }
        }

#endregion

        public readonly record struct PaletteEntry(Color Color, string Name);

        public readonly struct TilemapData
        {
            public required int TileWidth { get; init; }

            public required int TileHeight { get; init; }

            public required int BitsPerTile { get; init; }

            public required uint TileIDBitmask { get; init; }
            
            public required uint XFlipBitmask { get; init; }

            public required uint YFlipBitmask { get; init; }

            public required uint Rotation90Mask { get; init; }
        }
    }
}
