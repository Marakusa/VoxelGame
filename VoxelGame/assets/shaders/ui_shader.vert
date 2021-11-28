#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColorMultiplier;

out vec2 texCoord;
out vec4 colorMultiplier;

void main()
{
    texCoord = aTexCoord;
    colorMultiplier = vec4(aColorMultiplier.x, aColorMultiplier.y, aColorMultiplier.z, aColorMultiplier.w);

    gl_Position = vec4(position, 1.0);
}