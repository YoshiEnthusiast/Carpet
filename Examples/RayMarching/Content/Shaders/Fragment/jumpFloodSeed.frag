#version 460 core

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout (rgba32f, binding = 0) uniform image2D u_Input;
layout (rgba32f, binding = 1) uniform image2D u_Output;

void main()
{
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 norm = fragCoord / u_Resolution;

    int index = int(v_TexIndex);
    float alpha = texture(u_Textures[index], v_TexCoord).a;
    vec4 color = vec4(norm, 0., 1.) * step(1., alpha);

    o_FragColor = color;
}
