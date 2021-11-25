using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Game;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

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

        private Camera _playerCamera;

        private Shader _shader;
        private Texture _texture;
        private readonly Dictionary<Vector2, Chunk> _chunks = new();

        private const int ChunkWidth = 16, ChunkHeight = 128;

        private const int RowLength = 6;
        private const int RenderDistance = 1;

        private float[] _blockHighlightVertices =
        {
            // 0
            0f, 0f, 0f, 0f, 0f, 1f,
            // 1
            1f, 0f, 0f, 1f, 0f, 1f,
            // 2
            1f, 1f, 0f, 1f, 1f, 1f,
            // 3
            0f, 1f, 0f, 0f, 1f, 1f,
            
            // 4
            0f, 0f, 1f, 0f, 0f, 1f,
            // 5
            1f, 0f, 1f, 1f, 0f, 1f,
            // 6
            1f, 1f, 1f, 1f, 1f, 1f,
            // 7
            0f, 1f, 1f, 0f, 1f, 1f
        };
        private uint[] _blockHighlightIndices =
        {
            2, 1, 0,
            3, 2, 0,
            
            3, 0, 4,
            7, 3, 4,
            
            7, 4, 5,
            6, 7, 5,
            
            6, 5, 1,
            2, 6, 1,
            
            6, 2, 3,
            7, 6, 3,
            
            0, 1, 5,
            0, 5, 4,
        };
        private VertexBuffer _blockHighlightVb;
        private IndexBuffer _blockHighlightIb;
        private Vector3 _blockHighlightPoint = Vector3.Zero;
        
        protected override void OnLoad()
        {
            Noise.SetNoise(1f, 1f);
            Blocks.Initialize();

            _shader = new("assets/shader.vert", "assets/shader.frag");

            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            _playerCamera = new(new Vector3(8f, 65f, 8f));

            _texture = Texture.LoadFromFile("assets/atlas.png");
            _texture.Use(TextureUnit.Texture0);

            _shader.SetInt("texture0", 0);

            for (int x = -RenderDistance; x < RenderDistance; x++)
            {
                for (int z = -RenderDistance; z < RenderDistance; z++)
                {
                    Chunk chunk = new(x * ChunkWidth, z * ChunkWidth, ChunkWidth, ChunkHeight);
                    _chunks.Add(new(x * ChunkWidth, z * ChunkWidth), chunk);
                    chunk.Generate();
                }
            }

            _blockHighlightVb = new(_blockHighlightVertices, _blockHighlightVertices.Length * sizeof(float));
            _blockHighlightIb = new(_blockHighlightIndices, _blockHighlightIndices.Length * sizeof(uint));
            
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
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

            foreach (var chunk in _chunks)
            {
                Render(chunk.Value.Vb, chunk.Value.Ib, RowLength);
            }

            Render(_blockHighlightVb, _blockHighlightIb, RowLength);

            GL.UseProgram(0);
            
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Console.WriteLine($"[OpenGL Error] {error}");

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        private void Render(VertexBuffer vb, IndexBuffer ib, int rowLength)
        {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vb.Bind();
            ib.Bind();
            
            var positionLocation = _shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, rowLength * sizeof(float), 0);
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, rowLength * sizeof(float), 3 * sizeof(float));
            
            int colorMultiplier = _shader.GetAttribLocation("aColorMultiplier");
            GL.EnableVertexAttribArray(colorMultiplier);
            GL.VertexAttribPointer(colorMultiplier, 1, VertexAttribPointerType.Float, false, rowLength * sizeof(float), 5 * sizeof(float));
            
            GL.DrawElements(PrimitiveType.Triangles, ib.GetCount(), DrawElementsType.UnsignedInt, 0);

            GL.DeleteVertexArray(vao);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            
            vb.Unbind();
            ib.Unbind();
        }

        protected override void OnUnload()
        {
            CursorVisible = true;
            foreach (var chunk in _chunks)
            {
                chunk.Value.Vb.Delete();
                chunk.Value.Ib.Delete();
            }
            _blockHighlightVb.Delete();
            _blockHighlightIb.Delete();
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

            CheckBlockRaycast();

            if (input.IsKeyDown(Keys.Escape))
                Close();
            
            base.OnUpdateFrame(e);
        }

        private Vector3 _hitPoint = Vector3.NegativeInfinity;
        private Vector2 _chunkPoint = Vector2.NegativeInfinity;
        private Chunk _hitChunk;

        private Chunk GetChunkByPoint(Vector3 point)
        {
            Vector2 flooredPoint = new((float)Math.Floor(point.X / ChunkWidth) * ChunkWidth, (float)Math.Floor(point.Z / ChunkWidth) * ChunkWidth);
            if (_chunks.ContainsKey(flooredPoint))
            {
                return _chunks[flooredPoint];
            }

            return null;
        }
        private void CheckBlockRaycast()
        {
            _hitPoint = Vector3.NegativeInfinity;
            _hitChunk = null;
            _chunkPoint = Vector2.NegativeInfinity;
            
            Ray ray = new(_playerCamera.Position, _playerCamera.Front);

            for (; ray.GetLength() <= 6f; ray.Step())
            {
                Vector3 rayBlockPosition = ray.GetEndPoint();
                int x = (int)Math.Floor(rayBlockPosition.X);
                int y = (int)Math.Floor(rayBlockPosition.Y);
                int z = (int)Math.Floor(rayBlockPosition.Z);

                Chunk chunk = GetChunkByPoint(new(x, y, z));
                
                if (chunk != null && chunk.HasBlock((int)Math.Floor(x - chunk.Position.X), y, (int)Math.Floor(z - chunk.Position.Y)))
                {
                    x = (int)Math.Floor(x - chunk.Position.X);
                    z = (int)Math.Floor(z - chunk.Position.Y);
                    _hitPoint = new(x, y, z);
                    _hitChunk = chunk;
                    _chunkPoint = chunk.Position;
                    break;
                }
            }
            
            _blockHighlightPoint = new Vector3((float)Math.Floor(_hitPoint.X) + _chunkPoint.X, (float)Math.Floor(_hitPoint.Y),
                (float)Math.Floor(_hitPoint.Z) + _chunkPoint.Y);

            _blockHighlightVertices = new[]
            {
                // 0
                0f + _blockHighlightPoint.X, 0f + _blockHighlightPoint.Y, 0f + _blockHighlightPoint.Z, 0f, 0f, 1f,
                // 1
                1f + _blockHighlightPoint.X, 0f + _blockHighlightPoint.Y, 0f + _blockHighlightPoint.Z, 1f, 0f, 1f,
                // 2
                1f + _blockHighlightPoint.X, 1f + _blockHighlightPoint.Y, 0f + _blockHighlightPoint.Z, 1f, 1f, 1f,
                // 3
                0f + _blockHighlightPoint.X, 1f + _blockHighlightPoint.Y, 0f + _blockHighlightPoint.Z, 0f, 1f, 1f,

                // 4
                0f + _blockHighlightPoint.X, 0f + _blockHighlightPoint.Y, 1f + _blockHighlightPoint.Z, 0f, 0f, 1f,
                // 5
                1f + _blockHighlightPoint.X, 0f + _blockHighlightPoint.Y, 1f + _blockHighlightPoint.Z, 1f, 0f, 1f,
                // 6
                1f + _blockHighlightPoint.X, 1f + _blockHighlightPoint.Y, 1f + _blockHighlightPoint.Z, 1f, 1f, 1f,
                // 7
                0f + _blockHighlightPoint.X, 1f + _blockHighlightPoint.Y, 1f + _blockHighlightPoint.Z, 0f, 1f, 1f
            };

            _blockHighlightVb.SetBufferData(_blockHighlightVertices, _blockHighlightVertices.Length * sizeof(float));
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

        private bool _mouseDown;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (_playerCamera.IsLocked && !_mouseDown)
            {
                if (_hitChunk != null && _hitPoint != Vector3.NegativeInfinity)
                {
                    int x = (int)Math.Floor(_hitPoint.X);
                    int y = (int)Math.Floor(_hitPoint.Y);
                    int z = (int)Math.Floor(_hitPoint.Z);
                    _hitChunk.DestroyBlock(x, y, z);
                }
                
                _mouseDown = true;
            }
            
            _playerCamera.IsLocked = true;
            
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _mouseDown = false;
            
            base.OnMouseUp(e);
        }
    }
}