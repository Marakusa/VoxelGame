namespace VoxelGame.Game
{
    public class Chunk
    {
        private const int Width = 16, Height = 3;
        
        private Block[,,] _blocks;

        public Chunk()
        {
            _blocks = new Block[Width, Height, Width];
        }

        public void Generate()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        _blocks[x, y, z] = Blocks.Get("dirt");
                    }
                }   
            }
        }
    }
}