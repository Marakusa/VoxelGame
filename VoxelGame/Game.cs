using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace VoxelGame
{
    public class Game : GameWindow
    {
        public Game(int width, int height, string title) 
            : base(
                new(),
                new()
                {
                    Size = new OpenTK.Mathematics.Vector2i(width, height),
                    Title = title,
                    APIVersion = new System.Version(4, 6),
                    API = ContextAPI.OpenGL,
                    NumberOfSamples = 8,
                })
        { }

        public Camera PlayerCamera;
        
        private Shader _shader;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _incidesBufferObject;
        private int _elementBufferObject;

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
            
            -0.5f, 1.2f, 1.0f, 0.0f, 1.0f,
            -0.5f, 0.2f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            -0.5f, 1.2f, 1.0f, 0.0f, 1.0f,
            
            /*
            0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            0.5f,  -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f,  0.0f, 0.0f, 1.0f,*/
        };

        private float[] _incides =
        {
            // x     y     z    Texture(x, y)
            0, 1, 2,
            2, 3, 0,
            /*0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f,  0.0f, 0.0f, 1.0f,*/
        };
        
        protected override void OnLoad()
        {
            _shader = new("shader.vert", "shader.frag");
            
            GL.ClearColor(0.4f, 0.6f, 1.0f, 0.0f);

            PlayerCamera = new Camera();

            base.OnLoad();
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindVertexArray(_vertexArrayObject);
            GL.UseProgram(_shader.Handle);

            RenderTriangle();

            LoadTextures();

            Context.SwapBuffers();
            
            base.OnRenderFrame(e);
        }

        private void RenderTriangle()
        {
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            
            _incidesBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _incidesBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _incides.Length * sizeof(float), _incides, BufferUsageHint.StaticDraw);
            
            var positionLocation = GL.GetAttribLocation(_shader.Handle, "position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 0);
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            //int colorLocation = _shader.GetAttribLocation("aColor");
            //GL.EnableVertexAttribArray(colorLocation);
            //GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, _rowLength * sizeof(float), 3 * sizeof(float));

            /*_elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);*/
            
            //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / _rowLength);
        }

        private void LoadTextures()
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            Image<Rgba32> image = Image.Load<Rgba32>("dirt.png");
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pixels = new List<byte>(4 * image.Width * image.Height);
            
            for (int y = 0; y < image.Height; y++) {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++) {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            
            base.OnResize(e);
        }
        
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteBuffer(_incidesBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            _shader.Dispose();
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            PlayerCamera.Movement();
            PlayerCamera.Update();

            base.OnUpdateFrame(e);
        }
    }
}