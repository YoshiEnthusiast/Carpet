#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform vec4 u_ShadowBounds;
uniform float u_Mask;

void main()
{
    vec2 coord = (v_TexCoord - v_TexBounds.xy) / v_TexBounds.zw;
    vec2 shadowCoord = (u_ShadowBounds.xy + u_ShadowBounds.zw * coord) / 2240.;
    vec4 shadow = texture(u_Textures[0], shadowCoord);

    int mask = int(u_Mask);
    
    if (shadow[mask] > 0.)
        discard;

    float w = v_TexBounds.z;
    float r = w / 2.;
    float d = distance(v_TexCoord, v_TexBounds.xy + vec2(r, -r));

    float value = 1. - d / r;
    o_FragColor = vec4(v_Color.xyz * value, 1.);
}