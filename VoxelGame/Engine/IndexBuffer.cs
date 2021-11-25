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
            SetBufferData(data, count);
        }

        ~IndexBuffer()
        {
            Delete();
        }

        public void SetBufferData(uint[] data, int count)
        {
            _count = count;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _renderer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, count, data, BufferUsageHint.StaticDraw);
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
            GL.DeleteBuffers(1, ref _renderer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
