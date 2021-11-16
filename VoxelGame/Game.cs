using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
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

        private Shader _shader;
        
        private int _vertexBufferObject;

        private float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, -0.5f, 0.0f
        };
        
        protected override void OnLoad()
        {
            GL.ClearColor(1f, 0f, 1f, 1f);

            _vertexBufferObject = GL.GenBuffer();

            _shader = new("shader.vert", "shader.frag");

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
            GL.DeleteBuffer(_vertexBufferObject);
            _shader.Dispose();
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