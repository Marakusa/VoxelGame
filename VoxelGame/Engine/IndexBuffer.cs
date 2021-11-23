using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Engine
{
    public class IndexBuffer
    {
        private int _renderer;
        private int _count;

        public IndexBuffer(uint[] data, int count)
        {
            _count = count;
            GL.GenBuffers(1, out _renderer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _renderer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, count, data, BufferUsageHint.StaticDraw);
        }

        ~IndexBuffer()
        {
            Delete();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _renderer);
        }
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public int GetCount()
        {
            return _count;
        }

        public void Delete()
        {
            Unbind();
            GL.DeleteBuffers(1, ref _renderer);
            Dispose();
        }
        
        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Unbind();
                GL.DeleteBuffers(1, ref _renderer);
            }

            _renderer = 0;

            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
