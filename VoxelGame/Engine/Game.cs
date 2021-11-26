using System;
using System.Collections.Generic;
using System.IO;
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
        { }

        private Player _player;
        private Camera _playerCamera;

        private Shader _shader;
        private Shader _uiShader;
        private Texture _worldTexture;
        private Texture _uiTexture;
        
        private const int RowLength = 9;

        private HighlightBlock _highlightBlock;

        protected override void OnLoad()
        {
            Noise.SetNoise(1f, 1f);
            Blocks.Initialize();

            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            _player = new();
            _playerCamera = new(new Vector3(8f, 65f, 8f));

            InitializeShaders();
            
            _highlightBlock = new();
            
            //InitializeUI();
            
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

            ChunkManager.Initialize();

            base.OnLoad();
        }

        private void InitializeShaders()
        {
            _shader = new("assets/shaders/shader.vert", "assets/shaders/shader.frag");
            _worldTexture = Texture.LoadFromFile("assets/atlas.png", TextureUnit.Texture0);
            _worldTexture.Use(TextureUnit.Texture0);
            _shader.SetInt("texture0", 0);
            
            _uiShader = new("assets/shaders/ui_shader.vert", "assets/shaders/ui_shader.frag");
            _uiTexture = Texture.LoadFromFile("assets/textures/gui/player.png", TextureUnit.Texture1);
            _uiTexture.Use(TextureUnit.Texture1);
            _uiShader.SetInt("texture1", 1);
        }

        /*private VertexBuffer _uiVb;
        private IndexBuffer _uiIb;
        private float[] _uiTest =
        {
            0, 0, 0, 0, 1,
            0, 1, 0, 1, 1,
            1, 0, 1, 0, 1,
            1, 1, 1, 1, 1
        };
        private uint[] _uiTestIndices =
        {
            0, 1, 2,
            2, 1, 3
        };
        private void InitializeUI()
        {
            _uiVb = new(_uiTest, _uiTest.Length * sizeof(float));
            _uiIb = new(_uiTestIndices, _uiTestIndices.Length);
        }*/
        
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

            Dictionary<Vector2, Chunk> chunks = new (ChunkManager.GetChunks());
            foreach (var chunk in chunks)
            {
                if (chunk.Value.GetVertexBuffer() == null)
                    chunk.Value.SetBuffers();
                
                Render(chunk.Value.GetVertexBuffer(), chunk.Value.GetIndexBuffer());
            }

            if (_highlightBlock != null && _highlightBlock.mesh != null && _highlightBlock.mesh.Vb != null && _highlightBlock.mesh.Ib != null)
                Render(_highlightBlock.mesh.Vb, _highlightBlock.mesh.Ib);

            //GL.UseProgram(_uiShader.Handle);
            //Render(_uiVb, _uiIb, true);

            GL.UseProgram(0);
            
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Console.WriteLine($"[OpenGL Error] {error}");

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        private void Render(VertexBuffer vb, IndexBuffer ib)
        {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vb.Bind();
            ib.Bind();

            var positionLocation = _shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 0 * sizeof(float));
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 3 * sizeof(float));
            
            int colorMultiplierLocation = _shader.GetAttribLocation("aColorMultiplier");
            GL.EnableVertexAttribArray(colorMultiplierLocation);
            GL.VertexAttribPointer(colorMultiplierLocation, 4, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 5 * sizeof(float));

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
            ChunkManager.DeleteChunkBuffers();
            _highlightBlock.mesh.DeleteBuffers();
            _shader.Dispose();
            if (File.Exists("assets/atlas.png")) File.Delete("assets/atlas.png");
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!IsFocused) return;

            KeyboardState input = KeyboardState.GetSnapshot();

            _playerCamera.Movement(input, e);
            _playerCamera.Update();

            Vector2 currentChunk = new(_playerCamera.Position.X / ChunkManager.GetWidth(), _playerCamera.Position.Z / ChunkManager.GetWidth());

            if (currentChunk != _playerCamera.LastChunk)
            {
                _playerCamera.LastChunk = currentChunk;
                ChunkManager.LoadChunks(_playerCamera.Position);
            }

            CursorVisible = !_playerCamera.IsLocked;

            if (input.IsKeyDown(Keys.D1))
                _player?.SetCurrentSlot(0);
            else if (input.IsKeyDown(Keys.D2))
                _player?.SetCurrentSlot(1);
            else if (input.IsKeyDown(Keys.D3))
                _player?.SetCurrentSlot(2);
            else if (input.IsKeyDown(Keys.D4))
                _player?.SetCurrentSlot(3);
            else if (input.IsKeyDown(Keys.D5))
                _player?.SetCurrentSlot(4);
            else if (input.IsKeyDown(Keys.D6))
                _player?.SetCurrentSlot(5);
            else if (input.IsKeyDown(Keys.D7))
                _player?.SetCurrentSlot(6);
            else if (input.IsKeyDown(Keys.D8))
                _player?.SetCurrentSlot(7);
            else if (input.IsKeyDown(Keys.D9))
                _player?.SetCurrentSlot(8);

            CheckBlockRaycast();

            if (input.IsKeyDown(Keys.Escape))
                Close();
            
            base.OnUpdateFrame(e);
        }

        private Vector3 _hitPoint = Vector3.NegativeInfinity;
        private Vector3 _lastHitPoint = Vector3.NegativeInfinity;
        private Vector2 _chunkPoint = Vector2.NegativeInfinity;
        private Chunk _hitChunk;

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

                Chunk chunk = ChunkManager.GetChunkByPoint(new(x, y, z));
                
                if (chunk != null && chunk.HasBlock((int)Math.Floor(x - chunk.Position.X), y, (int)Math.Floor(z - chunk.Position.Y)))
                {
                    x = (int)Math.Floor(x - chunk.Position.X);
                    z = (int)Math.Floor(z - chunk.Position.Y);
                    
                    _hitPoint = new(x, y, z);
                    _lastHitPoint = ray.GetLastPoint();
                    _hitChunk = chunk;
                    _chunkPoint = chunk.Position;
                    
                    break;
                }
            }
            
            _highlightBlock.Position = new Vector3((float)Math.Floor(_hitPoint.X) + _chunkPoint.X, (float)Math.Floor(_hitPoint.Y), (float)Math.Floor(_hitPoint.Z) + _chunkPoint.Y);
            _highlightBlock.Generate();
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
                    if (e.Button == MouseButton.Left)
                    {
                        int x = (int)Math.Floor(_hitPoint.X);
                        int y = (int)Math.Floor(_hitPoint.Y);
                        int z = (int)Math.Floor(_hitPoint.Z);
                        _hitChunk.DestroyBlock(x, y, z);

                        // TODO: Fix neighbor chunk faces on break
                        /*var neighbor = ChunkManager.GetChunkByPoint(new(x - 1, y, z));
                        if (neighbor != null && neighbor != _hitChunk)
                            neighbor.RegenerateMesh();
                        
                        neighbor = ChunkManager.GetChunkByPoint(new(x + 1, y, z));
                        if (neighbor != null && neighbor != _hitChunk)
                            neighbor.RegenerateMesh();
                        
                        neighbor = ChunkManager.GetChunkByPoint(new(x, y, z - 1));
                        if (neighbor != null && neighbor != _hitChunk)
                            neighbor.RegenerateMesh();
                        
                        neighbor = ChunkManager.GetChunkByPoint(new(x, y, z + 1));
                        if (neighbor != null && neighbor != _hitChunk)
                            neighbor.RegenerateMesh();*/
                    }
                    else if (e.Button == MouseButton.Right)
                    {
                        // TODO: Fix blocks spawning opposite side if block placed next to another block at the borders of two chunks |^    #|_<--
                        int x = (int)Math.Floor(_lastHitPoint.X);
                        int y = (int)Math.Floor(_lastHitPoint.Y);
                        int z = (int)Math.Floor(_lastHitPoint.Z);

                        Chunk chunk = ChunkManager.GetChunkByPoint(new(x, y, z));
                
                        if (chunk != null)
                        {
                            int cx = (int)Math.Floor(x - chunk.Position.X);
                            int cz = (int)Math.Floor(z - chunk.Position.Y);

                            var item = _player.GetHotbarItem().item;
                            
                            if (item != null && item.GetType() == typeof(Block))
                                _hitChunk.PlaceBlock(cx, y, cz, (Block)item);
                        }
                    }
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