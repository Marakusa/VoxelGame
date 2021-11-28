namespace VoxelGame.Engine
{
    public class Renderer
    {
        // TODO: Use this instead of calling these in Game class
        public void Draw(VertexBuffer va, IndexBuffer ib, Shader shader)
        {
            shader.Bind();
            va.Bind();
            ib.Bind();
        }
    }
}
