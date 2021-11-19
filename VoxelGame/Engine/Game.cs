using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Game;

namespace VoxelGame.Engine
{
    public class Game : GameWindow
    {
        public Game(int width, int height, string title)
            : base(
                new(),
                new()
                {
                    Size = new Vector2i(width, height),
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
        private int _indicesBufferObject;

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

        private float[] _indices =
        {
            // x     y     z    Texture(x, y)
            0, 1, 2,
            2, 3, 0,
            /*0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f,  0.0f, 0.0f, 1.0f,*/
        };
        
        private Texture _texture;
        
        protected override void OnLoad()
        {
            var blocks = new Blocks();
            
            _shader = new("Resources/shader.vert", "Resources/shader.frag");
            
            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            PlayerCamera = new Camera();
            
            _texture = Texture.LoadFromFile("Resources/stone.png");
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
                var model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));
                var view = Matrix4.LookAt(PlayerCamera.Position, PlayerCamera.Position + PlayerCamera.Front, PlayerCamera.CameraUp);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(PlayerCamera.FieldOfView), x / y, 0.1f, 100.0f);

                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
            }

            Render();
            
            Context.SwapBuffers();
            
            base.OnRenderFrame(e);
        }

        private void Render()
        {
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            
            _indicesBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(float), _indices, BufferUsageHint.StaticDraw);
            
            var positionLocation = GL.GetAttribLocation(_shader.Handle, "position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 0);
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            //int colorLocation = _shader.GetAttribLocation("aColor");
            //GL.EnableVertexAttribArray(colorLocation);
            //GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / _rowLength);
            
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_indicesBufferObject);
            //GL.DeleteBuffer(_elementBufferObject);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.UseProgram(0);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            
            base.OnResize(e);
        }
        
        protected override void OnUnload()
        {
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