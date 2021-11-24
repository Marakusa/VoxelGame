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

        public Camera PlayerCamera;

        private Shader _shader;

        private Texture _texture;

        private List<Chunk> _chunks = new();

        public int RowLength = 6;

        public int RenderDistance = 1;

        protected override void OnLoad()
        {
            Noise noise = new(1f, 1f);
            Blocks blocks = new();

            _shader = new("assets/shader.vert", "assets/shader.frag");

            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            PlayerCamera = new(new Vector3(8f, 65f, 8f));
            //PlayerCamera = new(new Vector3(3f, 3f, 3f));

            _texture = Texture.LoadFromFile("assets/atlas.png");
            _texture.Use(TextureUnit.Texture0);

            _shader.SetInt("texture0", 0);

            //Chunk chunk = new(0, 0, 6, 2);
            //_chunks.Add(chunk);
            //chunk.Generate();
            for (int x = -RenderDistance; x < RenderDistance; x++)
            {
                for (int z = -RenderDistance; z < RenderDistance; z++)
                {
                    Chunk chunk = new(x * 16, z * 16, 16, 128);
                    _chunks.Add(chunk);
                    chunk.Generate();
                }
            }
            
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
                var view = Matrix4.LookAt(PlayerCamera.Position, PlayerCamera.Position + PlayerCamera.Front, PlayerCamera.CameraUp);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.FieldOfView), x / y, 0.1f, 100.0f);

                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
            }

            foreach (var chunk in _chunks)
            {
                Render(chunk);
            }

            GL.UseProgram(0);
            
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Console.WriteLine($"[OpenGL Error] {error}");

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        private int c = 0;
        
        private void Render(Chunk chunk)
        {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            if (c != chunk.Ib.GetCount())
            {
                c = chunk.Ib.GetCount();
                Console.WriteLine((chunk.Ib.GetCount() / sizeof(uint) / 6).ToString());
            }

            chunk.Vb.Bind();
            chunk.Ib.Bind();
            
            var positionLocation = GL.GetAttribLocation(_shader.Handle, "position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 0);
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 3 * sizeof(float));
            
            int colorMultiplier = _shader.GetAttribLocation("aColorMultiplier");
            GL.EnableVertexAttribArray(colorMultiplier);
            GL.VertexAttribPointer(colorMultiplier, 1, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 5 * sizeof(float));
            
            /*float[] camPos = new[]
            {
                PlayerCamera.Position.X,
                PlayerCamera.Position.Y,
                PlayerCamera.Position.Z
            };
            int cbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, camPos.Length * sizeof(float), camPos, BufferUsageHint.DynamicRead);

            int cameraPosition = _shader.GetAttribLocation("aCameraPosition");
            GL.EnableVertexAttribArray(cameraPosition);
            GL.VertexAttribPointer(cameraPosition, 3, VertexAttribPointerType.Float, false, camPos.Length * sizeof(float), camPos);*/
            
            GL.DrawElements(PrimitiveType.Triangles, chunk.Ib.GetCount(), DrawElementsType.UnsignedInt, 0);

            GL.DeleteVertexArray(vao);
            //GL.DeleteBuffer(cbo);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            
            chunk.Vb.Unbind();
            chunk.Ib.Unbind();
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
                    /*Console.WriteLine("------------------------------------");
                    Console.WriteLine(PlayerCamera.Position.ToString());
                    Console.WriteLine(PlayerCamera.Front.ToString());
                    Console.WriteLine("----------------");*/
                    Vector3 hit = chunk.CheckRaycastHitPoint(PlayerCamera.Position, PlayerCamera.Front, 10);
                    if (hit != Vector3.NegativeInfinity)
                    {
                        chunk.DestroyBlock(hit - new Vector3(chunk.Position.X, 0, chunk.Position.Y));
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
            foreach (var chunk in _chunks)
            {
                chunk.Vb.Delete();
                chunk.Ib.Delete();
            }
            CursorVisible = true;
            base.Close();
        }
    }
}