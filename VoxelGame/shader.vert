#version 330 core

layout (location = 0) in vec4 position;

layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;

void main()
{
    texCoord = aTexCoord;
    
    gl_Position = position;
}