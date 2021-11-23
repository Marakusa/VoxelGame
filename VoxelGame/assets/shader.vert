#version 330 core

layout (location = 0) in vec3 position;

layout (location = 1) in vec2 aTexCoord;

layout (location = 2) in float aColorMultiplier;

//layout (location = 3) in vec3 aCameraPosition;

out vec2 texCoord;
out vec4 colorMultiplier;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vec3 aCameraPosition = vec3(0, 0, 0);
    
    float xDist = aCameraPosition.x - position.x;
    float yDist = aCameraPosition.y - position.y;
    float zDist = aCameraPosition.z - position.z;

    float distanceToCamera = sqrt(pow(xDist, 2.0) + pow(yDist, 2.0) + pow(zDist, 2.0));
    distanceToCamera = clamp(distanceToCamera * 2, 0, 1);

    texCoord = aTexCoord;
    colorMultiplier = vec4(aColorMultiplier * distanceToCamera, aColorMultiplier * distanceToCamera, aColorMultiplier * distanceToCamera, 1.0);

    gl_Position = vec4(position, 1.0) * model * view * projection;
}