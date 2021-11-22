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
            Globals.Game = this;
        }

        public Camera PlayerCamera;

        private Shader _shader;

        private Texture _texture;

        private Dictionary<Vector2, Chunk> _chunks = new();

        private List<float> _chunkVertices = new();
        private List<int> _chunkIndices = new();

        

        public int VertexArrayObject;
        public int VertexBufferObject;
        public int IndicesBufferObject;

        public int Cbo;
        
        public int RowLength = 6;

        public float[] Vertices = Array.Empty<float>();
        public int[] Indices = Array.Empty<int>();
        
        
        
        protected override void OnLoad()
        {
            Noise noise = new(1f, 1f);
            Blocks blocks = new();

            _shader = new("assets/shader.vert", "assets/shader.frag");

            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            PlayerCamera = new();

            _texture = Texture.LoadFromFile("assets/atlas.png");
            _texture.Use(TextureUnit.Texture0);

            _shader.SetInt("texture0", 0);

            List<float> verticesList = new();
            List<int> indicesList = new();
            
            /*Chunk chunk = new(0, 0);
            
            chunk.Generated += (sender, vertices, indices) =>
            {
                verticesList.AddRange(vertices);
                indicesList.AddRange(indices);
                
                _vertices = verticesList.ToArray();
                _indices = indicesList.ToArray();
            };
            chunk.Generate();*/
            
            for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 5; z++)
                {
                    Chunk chunk = new(x * 16, z * 16, 16, 256);
                    chunk.Generated += (sender, vertices, indices) =>
                    {
                    };
                    _chunks.Add(new(chunk.Position.X, chunk.Position.Y), chunk);
                    chunk.Generate();
                }
            }
            
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            
            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _chunkVertices.Clear();
            _chunkIndices.Clear();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shader.Handle);

            float x = Size.X;
            float y = Size.Y;

            if (x > 0f && y > 0f)
            {
                var model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));
                var view = Matrix4.LookAt(PlayerCamera.Position, PlayerCamera.Position + PlayerCamera.Front, PlayerCamera.CameraUp);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.FieldOfView), x / y, 0.1f, 100.0f);

                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
            }

            foreach (var chunk in _chunks)
            {
                AddChunkToRender(chunk.Value);
            }

            Render();
            CleanRender();
            
            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        private void AddChunkToRender(Chunk chunk)
        {
            _chunkVertices.AddRange(chunk.Vertices);
            _chunkIndices.AddRange(chunk.Indices);
        }
        
        private void Render()
        {
            Vertices = _chunkVertices.ToArray();
            Indices = _chunkIndices.ToArray();
            
            float[] camPos = new[]
            {
                PlayerCamera.Position.X,
                PlayerCamera.Position.Y,
                PlayerCamera.Position.Z
            };
            
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
            
            IndicesBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(int), Indices, BufferUsageHint.StaticDraw);

            var positionLocation = GL.GetAttribLocation(_shader.Handle, "position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 0);

            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 3 * sizeof(float));

            int colorMultiplier = _shader.GetAttribLocation("aColorMultiplier");
            GL.EnableVertexAttribArray(colorMultiplier);
            GL.VertexAttribPointer(colorMultiplier, 1, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 5 * sizeof(float));

            Cbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, camPos.Length * sizeof(float), camPos, BufferUsageHint.DynamicRead);

            int cameraPosition = _shader.GetAttribLocation("aCameraPosition");
            GL.EnableVertexAttribArray(cameraPosition);
            GL.VertexAttribPointer(cameraPosition, 3, VertexAttribPointerType.Float, false, camPos.Length * sizeof(float), camPos);
       
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length / RowLength);
        }

        private void CleanRender()
        {
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteBuffer(IndicesBufferObject);
            GL.DeleteBuffer(Cbo);
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

            PlayerCamera.Movement(input, e);
            PlayerCamera.Update();

            CursorVisible = !PlayerCamera.IsLocked;

            if (input.IsKeyDown(Keys.Escape))
                Close();
            
            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (PlayerCamera.IsLocked)
            {
                PlayerCamera.Look(MousePosition);
                MousePosition = new(Size.X / 2f, Size.Y / 2f);
                PlayerCamera.LastMousePosition = new(Size.X / 2f, Size.Y / 2f);
                PlayerCamera.CurrentMousePosition = new(Size.X / 2f, Size.Y / 2f);
            }
            
            base.OnMouseMove(e);
        }

        private bool _mouseDown;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (PlayerCamera.IsLocked && !_mouseDown)
            {
                foreach (var chunk in _chunks)
                {
                    Console.WriteLine("------------------------------------");
                    Console.WriteLine(PlayerCamera.Position.ToString());
                    Console.WriteLine(PlayerCamera.Front.ToString());
                    Console.WriteLine("----------------");
                    Vector3 hit = chunk.Value.CheckRaycastHitPoint(PlayerCamera.Position, PlayerCamera.Front, 10);
                    if (hit != Vector3.NegativeInfinity)
                    {
                        chunk.Value.DestroyBlock(hit);
                    }
                }
                
                _mouseDown = true;
            }
            
            PlayerCamera.IsLocked = true;
            
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _mouseDown = false;
            
            base.OnMouseUp(e);
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