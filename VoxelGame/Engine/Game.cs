using System;
using System.Collections.Generic;
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
            : base(new(),
                new()
                {
                    Size = new Vector2i(width, height),
                    Title = title,
                    APIVersion = new(4, 6),
                    API = ContextAPI.OpenGL,
                    NumberOfSamples = 8,
                })
        {
        }

        private Camera _playerCamera;

        private Shader _shader;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _indicesBufferObject;

        private int _rowLength = 6;

        private float[] _vertices = Array.Empty<float>();
        private int[] _indices = Array.Empty<int>();

        private Texture _texture;

        protected override void OnLoad()
        {
            Noise noise = new(1f);
            Blocks blocks = new();

            _shader = new("resources/shader.vert", "resources/shader.frag");

            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            _playerCamera = new();

            _texture = Texture.LoadFromFile("resources/atlas.png");
            _texture.Use(TextureUnit.Texture0);

            _shader.SetInt("texture0", 0);

            List<float> verticesList = new();
            List<int> indicesList = new();
            
            Chunk chunk = new(0, 0);
            chunk.Generated += (sender, vertices, indices) =>
            {
                verticesList.AddRange(vertices);
                indicesList.AddRange(indices);
            };
            chunk.Generate();
            /*for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Chunk chunk = new(x * 16, y * 16);
                    chunk.Generated += (sender, vertices, indices) =>
                    {
                        verticesList.AddRange(vertices);
                        indicesList.AddRange(indices);
                    };
                    chunk.Generate();
                }
            }*/
            
            _vertices = verticesList.ToArray();
            _indices = indicesList.ToArray();

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shader.Handle);

            float x = Size.X;
            float y = Size.Y;

            if (x > 0f && y > 0f)
            {
                var model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));
                var view = Matrix4.LookAt(_playerCamera.Position, _playerCamera.Position + _playerCamera.Front, _playerCamera.CameraUp);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.FieldOfView), x / y, 0.1f, 100.0f);

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
                
            int _indicesBufferObjects = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBufferObjects);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(int), _indices, BufferUsageHint.StaticDraw);
            
            var positionLocation = GL.GetAttribLocation(_shader.Handle, "position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 0);

            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            int colorMultiplier = _shader.GetAttribLocation("aColorMultiplier");
            GL.EnableVertexAttribArray(colorMultiplier);
            GL.VertexAttribPointer(colorMultiplier, 1, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 5 * sizeof(float));

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_indicesBufferObject);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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

            _playerCamera.Movement(input, e);
            _playerCamera.Update();

            CursorVisible = !_playerCamera.IsLocked;

            if (input.IsKeyDown(Keys.Escape))
                Close();
            
            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (_playerCamera.IsLocked)
            {
                _playerCamera.Look(MousePosition);
                MousePosition = new(Size.X / 2f, Size.Y / 2f);
                _playerCamera.LastMousePosition = new(Size.X / 2f, Size.Y / 2f);
                _playerCamera.CurrentMousePosition = new(Size.X / 2f, Size.Y / 2f);
            }
            
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _playerCamera.IsLocked = true;
            
            base.OnMouseDown(e);
        }

        public override void Close()
        {
            CursorVisible = true;
            base.Close();
        }
    }

    public class MeshObject
    {
        public float[] Vertices = Array.Empty<float>();
        public int[] Indices = Array.Empty<int>();
    }
}