using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using GL = OpenTK.Graphics.OpenGL.GL;

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
        
        private int _vertexArrayObject;
        
        protected override void OnLoad()
        {
            GL.ClearColor(0f, 0f, 0f, 0f);

            _shader = new("shader.vert", "shader.frag");
            _shader.Use();

            DrawTriangle();
            
            base.OnLoad();
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            DrawTriangle();
            
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            _shader.Dispose();
            base.OnUnload();
        }

        private void DrawTriangle()
        {
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();

            GL.BindVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
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