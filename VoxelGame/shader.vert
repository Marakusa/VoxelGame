#version 330 core

layout (location = 0) in vec3 position;

layout (location = 1) in vec2 aTexCoord;

layout (location = 2) in vec4 aColor;

out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 color;

void main()
{
    texCoord = aTexCoord;
    color = aColor;
    gl_Position = vec4(position, 1.0) * model * view * projection;
}