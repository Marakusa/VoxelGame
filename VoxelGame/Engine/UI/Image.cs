using VoxelGame.Game;

namespace VoxelGame.Engine.UI
{
    public class Image : UIElement
    {
        private readonly UVTransform _uvTransform;

        public Image(UITransform transform, UVTransform uv, Shader shader) : base(shader, transform)
        {
            _uvTransform = uv;
            InitializeBuffers();
        }
    }
}
