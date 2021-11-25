#version 330 core

out vec4 outputColor;

in vec2 texCoord;
in vec4 colorMultiplier;

uniform sampler2D texture1;

void main()
{
    outputColor = texture(texture1, texCoord);
    outputColor.rgba *= colorMultiplier * vec4(1.0f, 0.5f, 0.2f, 1.0f);
}