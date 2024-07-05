#version 460 core

#define PI 3.1415926
#define TWO_PI 6.283185

#define MASKS_COUNT 4
#define GAUSSIAN_CENTRE .16
#define GAUSSIAN_COUNT 4

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform vec2 u_Resolution;

uniform float u_Index;

uniform float u_Rotation;
uniform float u_Angle;
uniform float u_FalloffAngle;

uniform float u_StartDistance;
uniform float u_StartFade;

uniform float u_Volume;

uniform bool u_Glare;

const float gaussian[] = {
    .15,
    .12,
    .09,
    .05
};

float deltaAngle(float source, float target)
{
    float result = target - source;

    while (result > PI)
        result -= TWO_PI;

    while (result < -PI)
        result += TWO_PI;

    return abs(result);
}

float sampleDistance(float x, float y, float r, int mask)
{
    vec4 c = texture(u_Textures[1], vec2(x, y));

    return step(r, c[mask]);
}

void main()
{
    float w = v_TexBounds.z;
    float r = w / 2.;
    vec2 center = v_TexBounds.xy + vec2(r, -r);

    vec2 delta = center - v_TexCoord;
    float d = length(delta);
    float a = atan(delta.y, delta.x) + PI;
    float normalized = d / r;

    int index = int(u_Index);
    int mask = index % MASKS_COUNT;

    vec2 distanceMapSize = textureSize(u_Textures[1], 0);
    vec2 distanceCoord = 1. - vec2(a / TWO_PI, floor(u_Index / 4.) / distanceMapSize.y);

    float blurOffset = (1. / distanceMapSize.x) * normalized; 

    float occluded = sampleDistance(distanceCoord.x, distanceCoord.y, 
        normalized, mask) * GAUSSIAN_CENTRE;

    for (int i = 0; i < GAUSSIAN_COUNT; i++)
        occluded += sampleDistance(distanceCoord.x - blurOffset * (i + 1.),
            distanceCoord.y, normalized, mask) * gaussian[i];

    for (int i = 0; i < GAUSSIAN_COUNT; i++)
        occluded += sampleDistance(distanceCoord.x + blurOffset * (i + 1.),
            distanceCoord.y, normalized, mask) * gaussian[i];

    // occluded += sampleDistance(distanceCoord.x - blurOffset * 4., distanceCoord.y,
    //     normalized, mask) * .05;
    // occluded += sampleDistance(distanceCoord.x - blurOffset * 3., distanceCoord.y,
    //     normalized, mask) * .09;
    // occluded += sampleDistance(distanceCoord.x - blurOffset * 2., distanceCoord.y,
    //     normalized, mask) * .12;
    // occluded += sampleDistance(distanceCoord.x - blurOffset * 1., distanceCoord.y,
    //     normalized, mask) * .15;

    // occluded += sampleDistance(distanceCoord.x + blurOffset * 4., distanceCoord.y,
    //     normalized, mask) * .05;
    // occluded += sampleDistance(distanceCoord.x + blurOffset * 3., distanceCoord.y,
    //     normalized, mask) * .09;
    // occluded += sampleDistance(distanceCoord.x + blurOffset * 2., distanceCoord.y,
    //     normalized, mask) * .12;
    // occluded += sampleDistance(distanceCoord.x + blurOffset * 1., distanceCoord.y,
    //     normalized, mask) * .15;

    if (!u_Glare)
    {
        vec2 occlusionMapSize = textureSize(u_Textures[0], 0);
        vec2 diff = (occlusionMapSize - u_Resolution) * .5;

        float occlusion = texture(u_Textures[0], 
                (gl_FragCoord.xy + diff)
                / occlusionMapSize).a;

        occluded *= (1. - occlusion);
    }
    
    float halfAngle = u_Angle / 2.;

    float rotation = mod(u_Rotation, TWO_PI);
    float angleDistance = deltaAngle(a, rotation);
    float angularFalloff = 1. - smoothstep(halfAngle - u_FalloffAngle, halfAngle, angleDistance);

    float distanceFalloff = max(1. - normalized, 0.);
    float startFalloff = smoothstep(u_StartDistance, u_StartDistance + u_StartFade, normalized);

    float v = startFalloff * distanceFalloff * angularFalloff
	    * occluded;

    o_FragColor = vec4(v_Color.xyz, u_Volume) * v;   
}
