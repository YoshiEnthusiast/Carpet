#version 460 core

layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec2 a_TexCoord;
layout (location = 2) in vec2 a_TexRes;
layout (location = 3) in vec4 a_TexBounds;
layout (location = 4) in vec4 a_Color;
layout (location = 5) in float a_TexIndex;

layout (std140) uniform Matrices
{
    mat4 u_Transform;
};

out vec2 v_TexCoord;
out vec2 v_TexRes;
out vec4 v_TexBounds;
out vec4 v_Color;
out float v_TexIndex;

void main()
{
    v_TexCoord = a_TexCoord;
    v_TexRes = a_TexRes;
    v_TexBounds = a_TexBounds;
    v_Color = a_Color;
    v_TexIndex = a_TexIndex;

    float depth = a_Position.z / 1000.;
    vec4 result = vec4(a_Position.xy, 0., 1.) * u_Transform;
    result.z = depth;
    gl_Position = result;
}