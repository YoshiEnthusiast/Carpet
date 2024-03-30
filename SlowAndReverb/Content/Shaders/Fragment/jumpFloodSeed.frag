#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform vec2 u_Resolution;

void main()
{
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 norm = fragCoord / u_Resolution;

    o_FragColor = vec4(norm, 1., 1.);
}