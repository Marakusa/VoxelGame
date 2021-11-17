#version 330 core

layout (location = 0) in vec3 position;

layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 proj = mat4(1);
uniform mat4 cam = mat4(1);
uniform mat4 obj = mat4(1);
uniform mat4 ani = mat4(1);

void main()
{
    texCoord = aTexCoord;
    gl_Position = proj * cam * obj * ani * vec4(position, 1.0);
}