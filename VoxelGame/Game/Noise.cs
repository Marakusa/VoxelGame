using System;
using System.Collections.Generic;

namespace VoxelGame.Game
{
    public class Noise
    {
        private static FastNoiseLite noise;

        private static float _noiseSize = 1f;
        
        public Noise(float noiseSize)
        {
            noise = new();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _noiseSize = noiseSize;
        }
        
        public static int GetNoise(int x, int y)
        {
            return (int)Math.Round(noise.GetNoise(x, y) * 5f + 60f);
        }
        
        public static int[,] GetChunkNoise(int locationX, int locationY)
        {
            int size = (int)Math.Round(128 / _noiseSize);
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
