using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelGame.Engine.UI
{
    public class UIManager
    {
        private UIElement[] _elements = Array.Empty<UIElement>();
        
        private readonly Shader _shader;
        private Texture _texture;

        public UIManager()
        {
            _shader = new("assets/shaders/ui_shader.vert", "assets/shaders/ui_shader.frag");
            _texture = Texture.LoadFromFile("assets/textures/gui/player.png", TextureUnit.Texture1);
            _texture.Use(TextureUnit.Texture1);
            _shader.SetInt("texture1", 1);
        }

        public void InitializeUI()
        {
            _elements = new[]
            {
                new Image(new() { Position = Vector2.Zero, Size = Vector2.One }, new(0f, 0f, 1f, 1f), _shader)
            };
        }

        public void RenderElements()
        {
            GL.UseProgram(_shader.Handle);
            
            foreach (var element in _elements)
            {
                element.Render();
            }
            
            GL.UseProgram(0);
        }

        public void Unload()
        {
            foreach (var element in _elements)
            {
                element.DeleteBuffers();
            }
            _shader.Dispose();
        }
    }
}
