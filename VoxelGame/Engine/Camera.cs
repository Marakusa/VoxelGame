using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Engine
{
    public class Camera
    {
        public const float FieldOfView = 70.0f;

        public Vector3 Position;
        public Vector3 CameraTarget;
        public Vector3 CameraDirection;

        public Vector3 Front;
        public Vector3 Up;
        public Vector3 CameraRight;
        public Vector3 CameraUp;

        private const float Speed = 15.0f;
        private const float Sensitivity = 0.1f;

        public Vector2 LastMousePosition, CurrentMousePosition;
        private float _pitch = 0f;
        private float _yaw = 0f;

        public bool IsLocked = true;
        private bool _cursorVisible = false;
        private bool _firstMouse = true;

        public Camera(Vector3 position)
        {
            Position = position;
            CameraTarget = Position + new Vector3(0.0f, 0.0f, 1.0f);
            CameraDirection = Vector3.Normalize(Position - CameraTarget);
            Front = new Vector3(0.0f, 0.0f, 1.0f);

            Update();
        }

        public void Update()
        {
            Up = Vector3.UnitY;
            CameraRight = Vector3.Normalize(Vector3.Cross(Up, CameraDirection));
            CameraUp = Vector3.Cross(CameraDirection, CameraRight);

            //Console.WriteLine(Position.ToString());
        }

        public void Look(Vector2 mousePosition)
        {
            if (_firstMouse)
            {
                LastMousePosition = new Vector2(mousePosition.X, mousePosition.Y);
                _firstMouse = false;
            }
            else
            {
                CurrentMousePosition = new Vector2(mousePosition.X, mousePosition.Y);
                
                float deltaX = CurrentMousePosition.X - LastMousePosition.X;
                float deltaY = CurrentMousePosition.Y - LastMousePosition.Y;
                LastMousePosition = CurrentMousePosition;

                _yaw += deltaX * Sensitivity;
                _pitch -= deltaY * Sensitivity;

                if (_pitch > 89.0f)
                    _pitch = 89.0f;
                else if (_pitch < -89.0f)
                    _pitch = -89.0f;

                Front.X = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_yaw));
                Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
                Front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_yaw));
                Front = Vector3.Normalize(Front);
            }
        }

        public void Movement(KeyboardState input, FrameEventArgs e)
        {
            if (input.IsKeyDown(Keys.W))
            {
                Position += Front * Speed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.S))
            {
                Position -= Front * Speed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.A))
            {
                Position -= Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.D))
            {
                Position += Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.Space))
            {
                Position += Up * Speed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                Position -= Up * Speed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.Escape))
            {
                IsLocked = false;
            }
        }
    }
}