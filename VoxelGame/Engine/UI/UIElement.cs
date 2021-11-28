using OpenTK.Graphics.OpenGL4;
using VoxelGame.Game;

namespace VoxelGame.Engine.UI
{
    public abstract class UIElement
    {
        public UITransform Transform;
        
        private VertexBuffer _uiVb;
        private IndexBuffer _uiIb;

        private readonly Shader _shader;

        protected UIElement(Shader shader, UITransform transform)
        {
            Transform = transform;
            _shader = shader;
        }

        private const int RowLength = 9;
        
        protected void InitializeBuffers()
        {
            _uiVb = new(new float[]
            {
                Transform.Position.X,                    Transform.Position.Y,                    0f, 0f, 1f, 1f, 1f, 1f, 1f,
                Transform.Position.X,                    Transform.Position.Y + Transform.Size.Y, 0f, 1f, 1f, 1f, 1f, 1f, 1f,
                Transform.Position.X + Transform.Size.X, Transform.Position.Y,                    0f, 0f, 1f, 1f, 1f, 1f, 1f,
                Transform.Position.X + Transform.Size.X, Transform.Position.Y + Transform.Size.Y, 0f, 1f, 1f, 1f, 1f, 1f, 1f
            }, RowLength * 4 * sizeof(float));
            
            _uiIb = new(new uint[]{
                0, 1, 2,
                2, 1, 3
            }, 6);
        }

        public void Render()
        {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            _uiVb.Bind();
            _uiIb.Bind();
            
            var positionLocation = _shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 0 * sizeof(float));
            
            int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 3 * sizeof(float));
            
            int colorMultiplierLocation = _shader.GetAttribLocation("aColorMultiplier");
            GL.EnableVertexAttribArray(colorMultiplierLocation);
            GL.VertexAttribPointer(colorMultiplierLocation, 4, VertexAttribPointerType.Float, false, RowLength * sizeof(float), 5 * sizeof(float));

            GL.DrawElements(PrimitiveType.Triangles, _uiIb.GetCount(), DrawElementsType.UnsignedInt, 0);

            GL.DeleteVertexArray(vao);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            
            UnbindBuffers();
        }

        public void DeleteBuffers()
        {
            _uiVb.Unbind();
            _uiIb.Unbind();
            _uiVb.Delete();
            _uiIb.Delete();
        }

        public void UnbindBuffers()
        {
            _uiVb.Unbind();
            _uiIb.Unbind();
        }
    }
}
