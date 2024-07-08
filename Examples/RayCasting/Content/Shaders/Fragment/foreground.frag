#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_Color;
in vec4 v_TexBounds;
in float v_TexIndex;

uniform sampler2D u_Textures[32];
uniform float u_Darkness;

layout (binding = 1) uniform sampler3D u_Palette;

void main()
{
    int index = int(v_TexIndex);
    vec4 color = texture(u_Textures[index], v_TexCoord) * v_Color;
    vec4 light = texture(u_Textures[0], v_TexCoord);

    vec4 result = color * light;

    float volume = light.w;
    result.xyz += light.xyz * volume;
    
    // To apply palette:
    // vec3 id = clamp(result.xyz, vec3(0.), vec3(.999));
    // result = texture(u_Palette, id);

    o_FragColor = vec4(result.xyz, color.a);
}
