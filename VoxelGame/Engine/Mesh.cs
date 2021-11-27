using System;

namespace VoxelGame.Engine
{
    public class Mesh
    {
        public float[] Vertices = Array.Empty<float>();
        public uint[] Indices = Array.Empty<uint>();

        public VertexBuffer Vb;
        public IndexBuffer Ib;

        ~Mesh()
        {
            DeleteBuffers();
        }

        public void SetData(float[] vertices, uint[] indices, EventHandler callback)
        {
            Vertices = vertices;
            Indices = indices;
            
            callback?.Invoke(this, EventArgs.Empty);
        }

        public void SetBuffers()
        {
            if (Vb == null)
                Vb = new VertexBuffer(Vertices, Vertices.Length * sizeof(float));
            else
            {
                Vb.Unbind();
                Vb.SetBufferData(Vertices, Vertices.Length * sizeof(float));
            }
            
            if (Ib == null)
                Ib = new IndexBuffer(Indices, Indices.Length * sizeof(uint));
            else
            {
                Ib.Unbind();
                Ib.SetBufferData(Indices, Indices.Length * sizeof(uint));
            }
        }
        public void SetBuffers(float[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;

            if (Vb == null)
                Vb = new VertexBuffer(Vertices, Vertices.Length * sizeof(float));
            else
            {
                Vb.Unbind();
                Vb.SetBufferData(Vertices, Vertices.Length * sizeof(float));
            }
            
            if (Ib == null)
                Ib = new IndexBuffer(Indices, Indices.Length * sizeof(uint));
            else
            {
                Ib.Unbind();
                Ib.SetBufferData(Indices, Indices.Length * sizeof(uint));
            }
        }

        public void DeleteBuffers()
        {
            Vb.Unbind();
            Ib.Unbind();
            Vb.Delete();
            Ib.Delete();
        }

        public void UnbindBuffers()
        {
            Vb.Unbind();
            Ib.Unbind();
        }
    }
}
