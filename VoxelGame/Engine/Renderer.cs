using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Engine
{
    public class Renderer
    {
        public void Draw(VertexBuffer va, IndexBuffer ib, Shader shader)
        {
            shader.Bind();
            va.Bind();
            ib.Bind();
        }
    }
}
