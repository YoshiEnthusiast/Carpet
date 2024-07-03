#version 460 core

#define PI 3.1415926
#define TWO_PI 6.283185

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform vec4 u_ShadowBounds;
uniform vec2 u_ShadowTexRes;
uniform float u_Mask;

uniform float u_Rotation;
uniform float u_Angle;
uniform float u_FalloffAngle;

uniform float u_StartDistance;
uniform float u_StartFade;

uniform float u_Volume;

uniform float u_ShadowFalloff;

float deltaAngle(float source, float target)
{
    float result = target - source;

    while (result > PI)
        result -= TWO_PI;

    while (result < -PI)
        result += TWO_PI;

    return abs(result);
}

void main()
{
    vec2 coord = (v_TexCoord - v_TexBounds.xy) / v_TexBounds.zw;
                                                        
    vec2 shadowCoord = (u_ShadowBounds.xy + u_ShadowBounds.zw * coord) / u_ShadowTexRes;  
    vec4 shadow = texture(u_Textures[0], shadowCoord);

    int mask = int(u_Mask);

    float w = v_TexBounds.z;
    float r = w / 2.;
    vec2 center = v_TexBounds.xy + vec2(r, -r);

    vec2 delta = center - v_TexCoord;
    float d = length(delta);
    float a = atan(delta.y, delta.x) + PI;
    
    float halfAngle = u_Angle / 2.;

    float rotation = mod(u_Rotation, TWO_PI);
    float angleDistance = deltaAngle(a, rotation);
    float angularFalloff = 1. - smoothstep(halfAngle - u_FalloffAngle, halfAngle, angleDistance);

    float normalized = d / r;
    float distanceFalloff = 1. - normalized;
    float startFalloff = smoothstep(u_StartDistance, u_StartDistance + u_StartFade, normalized);

    float pixelW = 1. / u_ShadowTexRes.x;
    float pixelH = 1. / u_ShadowTexRes.y;

    float c = 0.;

    for (int x = -1; x < 2; x++)
    {
        for (int y = -1; y < 2; y++)
        {
            coord = shadowCoord + vec2(x * pixelW, y * pixelH);

            vec4 neighbourShadow = texture(u_Textures[0], coord);
            vec4 occlusion = texture(u_Textures[1], coord);

            c += neighbourShadow[mask] * (1. - occlusion[mask]);
        }
    }

    float shadowFalloff = (1. - c / 6. * u_ShadowFalloff);
    shadowFalloff = 1.;

    float v = startFalloff * distanceFalloff * angularFalloff
        * shadowFalloff * (1. - shadow[mask]);

    o_FragColor = vec4(v_Color.xyz * v, u_Volume * v);   
}