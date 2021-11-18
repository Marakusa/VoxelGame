using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace VoxelGame
{
    public class Game : GameWindow
    {
        public Game(int width, int height, string title)
            : base(
                new(),
                new()
                {
                    Size = new OpenTK.Mathematics.Vector2i(width, height),
                    Title = title,
                    APIVersion = new Version(4, 6),
                    API = ContextAPI.OpenGL,
                    NumberOfSamples = 8,
                })
        { }

        public Camera PlayerCamera;
        
        private Shader _shader;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _incidesBufferObject;
        private int _elementBufferObject;

        private int _rowLength = 5;
        
        private float[] _vertices =
        {
            // x     y     z    Texture(x, y)
            0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            
            0.0f, 1.0f, -1.0f, 0.0f, 1.0f,
            0.0f, 0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 1.0f, -1.0f, 0.0f, 1.0f,
            
            1.0f, 1.0f, -1.0f, 0.0f, 1.0f,
            1.0f, 0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 1.0f, 0.0f,
            0.0f, 1.0f, -1.0f, 1.0f, 1.0f,
            0.0f, 0.0f, -1.0f, 1.0f, 0.0f,
            1.0f, 1.0f, -1.0f, 0.0f, 1.0f,
            
            /*
            0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            0.5f,  -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f,  0.0f, 0.0f, 1.0f,*/
        };

        private float[] _incides =
        {
            // x     y     z    Texture(x, y)
            0, 1, 2,
            2, 3, 0,
            /*0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f,  0.0f, 0.0f, 1.0f,*/
        };
        
        private Texture _texture;
        private Texture _texture2;
        
        protected override void OnLoad()
        {
            _shader = new("shader.vert", "shader.frag");
            
            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            PlayerCamera = new Camera();
            
            _texture = Texture.LoadFromFile("Resources/dirt.png");
            _texture.Use(TextureUnit.Texture0);

            _shader.SetInt("texture0", 0);
            
            base.OnLoad();
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindVertexArray(_vertexArrayObject);
            GL.UseProgram(_shader.Handle);

            float x = Size.X;
            float y = Size.Y;
            
            if (x > 0f && y > 0f)
            {
                Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));
                Matrix4 view = Matrix4.LookAt(PlayerCamera.Position, PlayerCamera.Position + PlayerCamera.Front, PlayerCamera.CameraUp);
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(PlayerCamera.FieldOfView), x / y, 0.1f, 100.0f);

                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
            }

            RenderTriangle();
            
            Context.SwapBuffers();
            
            base.OnRenderFrame(e);
        }

        private void RenderTriangle()
        {
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            
            _incidesBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _incidesBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _incides.Length * sizeof(float), _incides, BufferUsageHint.StaticDraw);
            
            var positionLocation = GL.GetAttribLocation(_shader.Handle, "position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 0);
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            //int colorLocation = _shader.GetAttribLocation("aColor");
            //GL.EnableVertexAttribArray(colorLocation);
            //GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            /*_elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);*/
            
            //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / _rowLength);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            
            base.OnResize(e);
        }
        
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteBuffer(_incidesBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            _shader.Dispose();
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!IsFocused) return;

            KeyboardState input = KeyboardState.GetSnapshot();

            PlayerCamera.Movement(input, e);
            PlayerCamera.Update();
            
            base.OnUpdateFrame(e);
        }
    }
}