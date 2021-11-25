#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aColorMultiplier;

out vec2 texCoord;
out vec4 colorMultiplier;

void main()
{
    texCoord = aTexCoord;
    colorMultiplier = vec4(aColorMultiplier, aColorMultiplier, aColorMultiplier, 1.0);

    gl_Position = vec4(position, 1.0);
}