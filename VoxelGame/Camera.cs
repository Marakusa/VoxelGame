using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 CameraTarget;
        public Vector3 CameraDirection;

        public Vector3 Front;
        public Vector3 Up;
        public Vector3 CameraRight;
        public Vector3 CameraUp;

        public float Speed = 1.5f;

        public Camera()
        {
            Position = new Vector3(0.0f, 0.0f, 3.0f);
            CameraTarget = new Vector3(0.0f, 0.0f, -1.0f);
            CameraDirection = Vector3.Normalize(Position - CameraTarget);
            
            Update();
        }

        public void Update()
        {
            Up = Vector3.UnitY;
            Front = new Vector3(0.0f, 0.0f, 1.0f);
            CameraRight = Vector3.Normalize(Vector3.Cross(Up, CameraDirection));
            CameraUp = Vector3.Cross(CameraDirection, CameraRight);
        }

        public void Movement(KeyboardState input, FrameEventArgs e)
        {
            if (input.IsKeyDown(Keys.W))
            {
                Position += Front * Speed * (float)e.Time; //Forward 
            }

            if (input.IsKeyDown(Keys.S))
            {
                Position -= Front * Speed * (float)e.Time; //Backwards
            }

            if (input.IsKeyDown(Keys.A))
            {
                Position -= Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * (float)e.Time; //Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                Position += Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * (float)e.Time; //Right
            }

            if (input.IsKeyDown(Keys.Space))
            {
                Position += Up * Speed * (float)e.Time; //Up 
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                Position -= Up * Speed * (float)e.Time; //Down
            }
            
            System.Console.WriteLine(Position.ToString());
        }
    }
}