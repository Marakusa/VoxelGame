using System;
using VoxelGame.Game;

namespace VoxelGame
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Voxel Game by Markus Kannisto");

            TextureManager textureManager = new();
            textureManager.GenerateTextureAtlas();
            
            //using Engine.Game game = new(1360, 786, "Voxel Game");
            //game.Run();
        }
    }
}