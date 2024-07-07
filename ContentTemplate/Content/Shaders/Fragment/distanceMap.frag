#version 460 core

#define PI 3.1415926
#define TWO_PI 6.283185

layout (location = 0) out vec4 o_FragColor;

in vec2 v_TexCoord;
in vec2 v_TexRes; 
in vec4 v_TexBounds;
in vec4 v_Color;
in float v_TexIndex;

uniform sampler2D u_Textures[32];

uniform vec2 u_LightPosition;
uniform float u_Radius;

void main()
{
	float angle = v_TexCoord.x * TWO_PI;
	float c = cos(angle);
	float s = sin(angle);

	vec2 size = textureSize(u_Textures[0], 0);

	float d = u_Radius;

	for (float r = 0.; r < u_Radius; r += 1.)
	{
		vec2 p = vec2(r * c, r * s) / size;
		p += u_LightPosition / size;

		vec4 occlusion = texture(u_Textures[0], vec2(p.x, 1. - p.y));

		if (occlusion.a > 0.)
			d = min(d, r);
	}

	d /= u_Radius;

	o_FragColor = vec4(v_Color * d);
}