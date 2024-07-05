#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform float u_Volume;

void main()
{
    float w = v_TexBounds.z;
    float r = w / 2.;
    vec2 center = v_TexBounds.xy + vec2(r, -r);

    vec2 delta = center - v_TexCoord;
    float d = length(delta);

    float distanceFalloff = max(1. - d / r, 0.);

    o_FragColor = vec4(v_Color.xyz, u_Volume) * distanceFalloff;       
}
