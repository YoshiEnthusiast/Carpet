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

uniform int u_RaysPerPixel;

uniform int u_MaxSteps;
uniform float u_SurfaceDistance;
uniform float u_MaxDistance;
uniform float u_Time;

const float SRGB_GAMMA = 1.0 / 2.2;

struct Hit 
{
    vec2 coord;
    float dist;
};

float random(vec2 st) 
{
   return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

// https://github.com/tobspr/GLSL-Color-Spaces/blob/master/ColorSpaces.inc.glsl
vec3 rgb_to_srgb_approx(vec3 rgb) {
    return pow(rgb, vec3(SRGB_GAMMA));
}

bool rayMarch(vec2 rayOrigin, vec2 rayDirection, out Hit hit)
{
    float totalDist = 0.;

    for (int i = 0; i < u_MaxSteps; i++)
    {
        vec2 coord = rayOrigin + rayDirection * totalDist;

        if (coord.x > 1. || coord.x < 0. || coord.y > 1. || coord.y < 0.)
            return false;

        float dist = texture(u_Textures[1], coord).r;

        totalDist += dist;

        if (dist <= 0.001)
        {
            hit = Hit(coord, totalDist);

            return true;
        }
        
        if (totalDist >= u_MaxDistance)
            return false;
    }

    return false;
}

void main()
{
    float emissive;
    vec3 color;

    vec2 size = textureSize(u_Textures[0], 0);
    vec2 rayOrigin = v_TexCoord;

    for (int i = 0; i < u_RaysPerPixel; i++)
    {
        float angle = random(v_TexCoord + vec2(float(i))) * TWO_PI + u_Time;
        vec2 rayDirection = normalize(vec2(cos(angle), sin(angle)));

        Hit hit;

        if (rayMarch(rayOrigin, rayDirection, hit))
        {
            vec4 data = texture(u_Textures[0], hit.coord);
            float dist = hit.dist;

            emissive += max(max(data.r, data.g), data.b);
            color += data.rgb * (1. - dist);
        }
    }

    color /= emissive;
    emissive /= float(u_RaysPerPixel);

    vec3 result = rgb_to_srgb_approx(color * emissive);

    o_FragColor = vec4(result, 1.);
}