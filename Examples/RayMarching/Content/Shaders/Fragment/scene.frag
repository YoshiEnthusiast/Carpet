#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_Color;
in vec4 v_TexBounds;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

void main()
{
    int index = int(v_TexIndex);
    vec4 color = texture(u_Textures[index], v_TexCoord);
    vec4 light = texture(u_Textures[0], v_TexCoord);


    o_FragColor = color + light;

    if (isnan(light.g))
        o_FragColor = vec4(1., 0., 0., 1.);
}
