using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using VoxelGame.Engine.UI;
using VoxelGame.Game;

namespace VoxelGame.Engine
{
    public class Renderer
    {
        private UIManager _uiManager;
        
        private const int RowLength = 9;
        private Shader _shader;
        private Texture _worldTexture;

        public readonly HighlightBlock Highlight;

        private Player _player;
        private Camera _playerCamera;

        public Renderer()
        {
            _uiManager = new();
            Highlight = new();
        }

        // TODO: Use this instead of calling these in Game class
        public void Draw(VertexBuffer va, IndexBuffer ib, Shader shader)
        {
            shader.Bind();
            va.Bind();
            ib.Bind();
        }

        public void Load(Player player, Camera camera)
        {
            _player = player;
            _playerCamera = camera;
            
            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            InitializeShaders();
            
            _uiManager.InitializeUI();
            
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

            ChunkManager.Initialize();
        }

        public void RenderFrame(GameWindow window)
        {
            GL.Viewport(0, 0, window.Size.X, window.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shader.Handle);

            float x = window.Size.X;
            float y = window.Size.Y;

            if (x > 0f && y > 0f)
            {
                var model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));
                var view = Matrix4.LookAt(_playerCamera.Position, _playerCamera.Position + _playerCamera.Front, _playerCamera.CameraUp);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.FieldOfView), x / y, 0.1f, 1000.0f);

                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
            }

            Dictionary<Vector2, Chunk> chunks = new (ChunkManager.GetChunks());
            foreach (var chunk in chunks)
            {
                if (chunk.Value.IsGenerated)
                {
                    if (chunk.Value.GetVertexBuffer() == null)
                        chunk.Value.SetBuffers();

                    Render(chunk.Value.GetVertexBuffer(), chunk.Value.GetIndexBuffer());
                }
            }

            if (Highlight != null && Highlight.mesh != null && Highlight.mesh.Vb != null && Highlight.mesh.Ib != null)
                Render(Highlight.mesh.Vb, Highlight.mesh.Ib);
            
            GL.UseProgram(0);

            _uiManager.RenderElements();
            
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Console.WriteLine($"[OpenGL Error] {error}");
        }
    
        private void InitializeShaders()
        {
            _shader = new("assets/shaders/shader.vert", "assets/shaders/shader.frag");
            _worldTexture = Texture.LoadFromFile("assets/atlas.png", TextureUnit.Texture0);
            _worldTexture.Use(TextureUnit.Texture0);
            _shader.SetInt("texture0", 0);
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

        public void Unload()
        {
            Highlight.mesh.DeleteBuffers();
            _shader.Dispose();
            _uiManager.Unload();
        }
    }
}
