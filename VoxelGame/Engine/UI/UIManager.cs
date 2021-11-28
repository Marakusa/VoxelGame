namespace VoxelGame.Engine.UI
{
    public class UIManager
    {
        private Shader _uiShader;
        private Texture _uiTexture;

        private VertexBuffer _uiVb;
        private IndexBuffer _uiIb;
        private float[] _uiTest =
        {
            0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f,
            0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f,
            1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f,
            1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f
        };
        private uint[] _uiTestIndices =
        {
            0, 1, 2,
            2, 1, 3
        };

        public void InitializeUI()
        {
            _uiVb = new(_uiTest, _uiTest.Length * sizeof(float));
            _uiIb = new(_uiTestIndices, _uiTestIndices.Length);
        }
    }
}
