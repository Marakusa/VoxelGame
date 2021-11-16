using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace VoxelGame
{
    public class Shader
    {
        private int handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexShaderSource;

            using (StreamReader reader = new(vertexPath, Encoding.UTF8))
            {
                vertexShaderSource = reader.ReadToEnd();
            }

            string fragmentShaderSource;

            using (StreamReader reader = new(fragmentPath, Encoding.UTF8))
            {
                fragmentShaderSource = reader.ReadToEnd();
            }

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            
            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);

            string infoLogVertex = GL.GetShaderInfoLog(vertexShader);
            if (infoLogVertex != String.Empty)
                Console.WriteLine(infoLogVertex);

            string infoLogFragment = GL.GetShaderInfoLog(fragmentShader);
            if (infoLogFragment != String.Empty)
                Console.WriteLine(infoLogFragment);

            handle = GL.CreateProgram();
            
            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);
            
            GL.LinkProgram(handle);
            
            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}