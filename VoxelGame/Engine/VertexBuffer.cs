using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Engine
{
    public class VertexBuffer
    {
        private int _renderer;

        public VertexBuffer(float[] data, int size)
        {
            GL.GenBuffers(1, out _renderer);
            SetBufferData(data, size);
        }

        ~VertexBuffer()
        {
            Delete();
        }

        public void SetBufferData(float[] data, int size)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _renderer);
            GL.BufferData(BufferTarget.ArrayBuffer, size, data, BufferUsageHint.StaticDraw);
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
            GL.DeleteBuffers(1, ref _renderer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
