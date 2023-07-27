#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_Color;
in vec4 v_TexBounds;
in float v_TexIndex;

uniform sampler2D u_Textures[32];
uniform vec2 u_Scale;

float modulo(float a, float b)
{
    while (a > b)
        a -= b;

    return a;
}

void main()
{
    float boundsWidth = v_TexBounds.z;
    float boundsHeight = v_TexBounds.w;

    vec2 topLeft = v_TexBounds.xy;
    vec2 relCoord = v_TexCoord - topLeft;
    vec2 scaledCoord = relCoord * u_Scale;

    float w = modulo(scaledCoord.x, boundsWidth);
    float h = modulo(scaledCoord.y, boundsHeight);

    vec2 coord = topLeft + vec2(w, h);
    
    int index = int(v_TexIndex);
    o_FragColor = texture(u_Textures[index], coord);
}