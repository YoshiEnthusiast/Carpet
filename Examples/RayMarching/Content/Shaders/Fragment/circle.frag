#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform float u_Width;
uniform float u_Circumference;

void main()
{
    float w = v_TexBounds.z;
    float r = w / 2.;
    float d = distance(v_TexCoord, v_TexBounds.xy + vec2(r, -r));

    if (d > r || d < r - v_TexBounds.z / u_Circumference * u_Width)
        discard;

    o_FragColor = v_Color;
}
