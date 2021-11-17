using System;

namespace VoxelGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Voxel Game by Markus Kannisto");
            
            using (Game game = new(1360, 786, "Voxel Game"))
            {
                game.Run();
            }
        }
    }
}