using OpenTK.Mathematics;

namespace VoxelGame.Engine
{
    public class Ray
    {
        private readonly Vector3 _start;
        private readonly Vector3 _direction;

        private readonly float _stepLength;
        private float _currentStep;
        
        public Ray(Vector3 start, Vector3 direction, float stepLength = 1f)
        {
            direction.Normalize();
            _start = start;
            _direction = direction;
            _stepLength = stepLength;
        }

        public float GetLength()
        {
            return Vector3.Distance(_start, GetEndPoint());
        }
        public Vector3 GetEndPoint()
        {
            return _start + _direction * _currentStep;
        }

        public void Step()
        {
            _currentStep += _stepLength;
        }

        /*public Vector3 CheckRaycastHitPoint(Vector3 start, Vector3 direction, float length)
        {
            direction.Normalize();
            Vector3 end = direction * length;
            
            //Console.WriteLine("----------------");
            //Console.WriteLine(start.ToString());
            //Console.WriteLine(new Vector3((float)Math.Round(direction.X), (float)Math.Round(direction.Y), (float)Math.Round(direction.Z)).ToString());
            //Console.WriteLine(direction.ToString());

            int mapX = (int)start.X;
            int mapY = (int)start.Y;
            int mapZ = (int)start.Z;
            
            double toDistX;
            double toDistY;
            double toDistZ;
            
            double deltaDistX = (direction.X == 0) ? 1e30 : (1 / direction.X);
            double deltaDistY = (direction.Y == 0) ? 1e30 : (1 / direction.Y);
            double deltaDistZ = (direction.Z == 0) ? 1e30 : (1 / direction.Z);
            
            int stepX;
            int stepY;
            int stepZ;

            bool hit = false;
            
            if (direction.X < 0)
            {
                stepX = -1;
                toDistX = ((int)start.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                toDistX = (mapX + 1.0 - (int)start.X) * deltaDistX;
            }
            
            if (direction.Y < 0)
            {
                stepY = -1;
                toDistY = ((int)start.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                toDistY = (mapY + 1.0 - (int)start.Y) * deltaDistY;
            }
            
            if (direction.Z < 0)
            {
                stepZ = -1;
                toDistZ = ((int)start.Z - mapZ) * deltaDistZ;
            }
            else
            {
                stepZ = 1;
                toDistZ = (mapZ + 1.0 - (int)start.Z) * deltaDistZ;
            }
            
            while (!hit)
            {
                if (toDistX < toDistY && toDistX < toDistZ)
                {
                    toDistX += deltaDistX;
                    mapX += stepX;
                }
                else if (toDistY < toDistZ)
                {
                    toDistY += deltaDistY;
                    mapY += stepY;
                }
                else
                {
                    toDistZ += deltaDistZ;
                    mapZ += stepZ;
                }

                if (Vector3.Distance(start, new(mapX, mapY, mapZ)) > length)
                {
                    return Vector3.NegativeInfinity;
                }
                
                //Console.WriteLine("==");
                //Console.WriteLine(new Vector3(mapX, mapY, mapZ).ToString());

                if (mapX >= Position.X && mapY >= 0 && mapZ >= Position.Y
                    && mapX < Position.X + Width && mapY < Height && mapZ < Position.Y + Width)
                {
                    if (_blocks[mapX - (int)Position.X, mapY, mapZ - (int)Position.Y] != null)
                    {
                        hit = true;
                    }
                }
            }
            
            return new(mapX, mapY, mapZ);
        }*/
    }
}
