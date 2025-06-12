using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Engine
{
    public class Camera
    {
        public const float FieldOfView = 70.0f;

        public Vector2 LastChunk = Vector2.NegativeInfinity;
        
        public Vector3 Position;
        public Vector3 CameraTarget;
        public Vector3 CameraDirection;

        public Vector3 Front;
        public Vector3 Up;
        public Vector3 CameraRight;
        public Vector3 CameraUp;

        private const float Speed = 15.0f;
        private const float Sensitivity = 0.1f;

        private Vector2 _lastMousePosition, _currentMousePosition;
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

        public void UpdateMouseLock(Vector2 value)
        {
            value = new Vector2(MathF.Floor(value.X), MathF.Floor(value.Y));
            _lastMousePosition = value;
            _currentMousePosition = value;
        }

        public void Look(Vector2 mousePosition)
        {
            if (_firstMouse)
            {
                _lastMousePosition = new Vector2(MathF.Floor(mousePosition.X), MathF.Floor(mousePosition.Y));
                _firstMouse = false;
                return;
            }

            _currentMousePosition = new Vector2(MathF.Floor(mousePosition.X), MathF.Floor(mousePosition.Y));
            Console.WriteLine(mousePosition + " : " + _currentMousePosition);

            float deltaX = _currentMousePosition.X - _lastMousePosition.X;
            float deltaY = _currentMousePosition.Y - _lastMousePosition.Y;
            Console.WriteLine(_lastMousePosition + " > " + _currentMousePosition);
            _lastMousePosition = _currentMousePosition;

            Console.WriteLine(deltaX + ", " + deltaY);

            _yaw += deltaX * Sensitivity;
            _pitch -= deltaY * Sensitivity;

            _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);

            float yawRad = MathHelper.DegreesToRadians(_yaw);
            float pitchRad = MathHelper.DegreesToRadians(_pitch);

            Front = new Vector3(
                (float)(Math.Cos(pitchRad) * Math.Cos(yawRad)),
                (float)Math.Sin(pitchRad),
                (float)(Math.Cos(pitchRad) * Math.Sin(yawRad))
            );
            Front = Vector3.Normalize(Front);
        }

        public void Movement(FrameEventArgs e)
        {
            if (Input.IsKeyDown(Keys.W))
            {
                Position += Front * Speed * (float)e.Time;
            }

            if (Input.IsKeyDown(Keys.S))
            {
                Position -= Front * Speed * (float)e.Time;
            }

            if (Input.IsKeyDown(Keys.A))
            {
                Position -= Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * (float)e.Time;
            }

            if (Input.IsKeyDown(Keys.D))
            {
                Position += Vector3.Normalize(Vector3.Cross(Front, Up)) * Speed * (float)e.Time;
            }

            if (Input.IsKeyDown(Keys.Space))
            {
                Position += Up * Speed * (float)e.Time;
            }

            if (Input.IsKeyDown(Keys.LeftShift))
            {
                Position -= Up * Speed * (float)e.Time;
            }

            if (Input.IsKeyDown(Keys.Escape))
            {
                IsLocked = false;
            }
        }
    }
}