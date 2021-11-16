using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using All = OpenTK.Graphics.ES11.All;
using BufferTargetArb = OpenTK.Graphics.ES11.BufferTargetArb;
using ClearBufferMask = OpenTK.Graphics.ES11.ClearBufferMask;
using GL = OpenTK.Graphics.ES11.GL;

namespace VoxelGame
{
    public class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(new()
        {
            IsMultiThreaded = true
        }, NativeWindowSettings.Default)
        {
            Size = new(width, height);
            Title = title;
        }

        private Shader Shader;
        
        private int vertexBufferObject;

        private float[] vertices =
        {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, -0.5f, 0.0f
        };
        
        protected override void OnLoad()
        {
            GL.ClearColor(1f, 0f, 1f, 1f);

            vertexBufferObject = GL.GenBuffer();

            Shader = new("shader.vert", "shader.frag");

            base.OnLoad();
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Context.SwapBuffers();
            
            base.OnRenderFrame(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            
            base.OnResize(e);
        }
        
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTargetArb.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferObject);
            Shader.Dispose();
            base.OnUnload();
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            /*KeyboardState input = KeyboardState.GetSnapshot();

            if (input.IsKeyDown(Keys.Escape))
            {
                Console.WriteLine("Escape");
            }*/

            base.OnUpdateFrame(e);
        }
    }
}