#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes; //TODO: use textureSize() instead
in vec4 v_Color;
in vec4 v_TexBounds;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

void main()
{
    int index = int(v_TexIndex);
    o_FragColor = texture(u_Textures[index], v_TexCoord) * v_Color;
}