#version 460 core

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes;
in vec4 v_Color;
in vec4 v_TexBounds;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform vec4 u_OutlineColor;
uniform int u_OutlineWidth;

void main()
{
    int index = int(v_TexIndex);
    vec4 texCol = texture(u_Textures[index], v_TexCoord);

    if (texCol.a > 0)
    {
        o_FragColor = v_Color;
    }
    else
    {
        vec2 uv = v_TexCoord;

        float pixelWidth = 1 / v_TexRes.x;
        float pixelHeight = 1 / v_TexRes.y;

        for (int x = -u_OutlineWidth; x <= u_OutlineWidth; x++)
        {
            for (int y = -u_OutlineWidth; y <= u_OutlineWidth; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                float newX = uv.x + x * pixelWidth;
                float newY = uv.y + y * pixelHeight;

                if (newX > 1 || newX < 0 || newY > 1 || newY < 0)
                    continue;

                if (texture(u_Textures[index], vec2(newX, newY)).a > 0)
                {
                    o_FragColor = u_OutlineColor;

                    return;
                }
            }
        }

        discard;
    }
}