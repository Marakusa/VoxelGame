using System;

namespace VoxelGame.Game
{
    public static class Noise
    {
        private static FastNoiseLite _noise;

        private static float _noiseFreq = 1f;
        private static float _noiseSize = 1f;
        
        public static void SetNoise(float frequency, float size)
        {
            _noise = new();
            _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _noiseFreq = frequency;
            _noiseSize = size;
        }
        
        public static int GetNoise(int x, int y)
        {
            return (int)Math.Round(_noise.GetNoise(x, y) * 5f * _noiseSize + 60f);
        }
        
        public static int[,] GetChunkNoise(int locationX, int locationY)
        {
            int size = (int)Math.Round(128 * _noiseFreq);
            int[,] noiseData = new int[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    noiseData[x, y] = GetNoise(x + locationX, y + locationY);
                }
            }

            return noiseData;
        }
        
        /*public static List<int[]> GenerateWormNoise(int startLocationX, int startLocationY, int length)
        {
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            int size = (int)Math.Round(128 / _noiseSize);
            List<int[]> noiseData = new();

            int wormX = startLocationX;
            int wormY = startLocationY;

            for (int x = 0; x < size; x++)
            {
                float noiseValueX = noise.GetNoise(x + startLocationX, startLocationY);
                float noiseValueY = noise.GetNoise(x + startLocationX, startLocationY);
                float noiseValueZ = noise.GetNoise(x + startLocationX, startLocationY);
                noiseData.Add(new[] { wormX, wormY });
            }

            return noiseData;
        }*/
    }
}
