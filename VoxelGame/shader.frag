#version 330 core

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;

in vec4 color;

void main()
{
    outputColor = texture(texture0, texCoord);
    //outputColor = color;
}