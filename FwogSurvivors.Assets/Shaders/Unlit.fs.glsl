#version 460 core

layout(location = 1) in vec3 v_color;
layout(location = 2) in vec2 v_uv;

layout(location = 0) out vec4 o_color; 

layout(binding = 0) uniform sampler2D s_frog;

void main()
{
    o_color = texture(s_frog, v_uv) * vec4(v_color, 1.0);
}