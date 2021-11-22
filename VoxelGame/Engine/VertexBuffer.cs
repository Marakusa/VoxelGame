using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Engine
{
    public class VertexBuffer
    {
        private int _renderer;

        public VertexBuffer(float[] data, int size)
        {
            _renderer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _renderer);
            GL.BufferData(BufferTarget.ArrayBuffer, size, data, BufferUsageHint.StaticDraw);
        }

        ~VertexBuffer()
        {
            Delete();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _renderer);
        }
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        {
            Unbind();
            GL.DeleteBuffer(_renderer);
        }
    }
}
