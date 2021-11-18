using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 CameraTarget;
        public Vector3 CameraDirection;

        public Vector3 Up;
        public Vector3 CameraRight;
        public Vector3 CameraUp;

      public  float Speed = 1.5f;

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
            CameraRight = Vector3.Normalize(Vector3.Cross(Up, CameraDirection));
            CameraUp = Vector3.Cross(CameraDirection, CameraRight);

            Matrix4 view = Matrix4.LookAt(Position, CameraTarget, CameraUp);
        }

        public void Movement()
        {KeyboardState input = KeyboardState.GetSnapshot();

            if (input.IsKeyDown(Keys.Escape))
            {
                Console.WriteLine("Escape");
            }
            if (!Focused) // check to see if the window is focused
            {
                return;
            }

            KeyboardState input = Keyboard.GetState();

            //...

            if (input.IsKeyDown(Key.W))
            {
                position += front * speed; //Forward 
            }

            if (input.IsKeyDown(Key.S))
            {
                position -= front * speed; //Backwards
            }

            if (input.IsKeyDown(Key.A))
            {
                position -= Vector3.Normalize(Vector3.Cross(front, up)) * speed; //Left
            }

            if (input.IsKeyDown(Key.D))
            {
                position += Vector3.Normalize(Vector3.Cross(front, up)) * speed; //Right
            }

            if (input.IsKeyDown(Key.Space))
            {
                position += up * speed; //Up 
            }

            if (input.IsKeyDown(Key.LShift))
            {
                position -= up * speed; //Down
            }
        }
    }
}