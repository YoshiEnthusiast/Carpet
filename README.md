# Carpet

2D game framework with an emphasis on lighting made with [OpenTK](https://github.com/opentk/opentk).

## Main features

### Graphics

1. **OpenGL as backend.**
2. **GPU and CPU Ray Casting.** Individual lights have configurable width, angle, 
angular and shadow falloff and volume. There is also bloom — light that is not affected
by any occluders. Occluders can have any shape.

3. **Ray Marched lighting** with an option to use displacement textures, which set a particular
starting rotation to the rays emitted.

4. **Sprite batching.**
5. **Sprite atlas generation.**
6. **Limited palettes.**
7. **Font Rendering.** Bitmap fonts are generated via [Hiero](https://libgdx.com/wiki/tools/hiero). 
The output `.fnt` file is converted to `.xml` using [this script](https://gist.github.com/kleber-swf/83cb74fe999b10c524aa352e53752ee6)

### Pixel perfect pyhsics

### Audio

**Audio using OpenAL** that offers both short sound playback and music streaming.

### Misc

1. **Particle system.**
2. **Debug console.**
3. **Coroutines.**
4. **Pseudo-ECS.**

## NuGet packages used

* [OpenTK](https://www.nuget.org/packages/OpenTK/5.0.0-pre.10) (Version: 4,7,7)
* [StbImageSharp](https://www.nuget.org/packages/StbImageSharp) (Version: 2.27.13)
* [StbImageWriteSharp](https://www.nuget.org/packages/StbImageWriteSharp) (Version: 1.16.7)

## Running examples

```
git clone https://github.com/YoshiEnthusiast/Carpet.git
cd Carpet
dotnet run --project ./Examples/[Project name] --configuration Release
```

## Lessons learnt

"Make games, not engines".\
Extensive abstractions increase complexity and limit functionality.
