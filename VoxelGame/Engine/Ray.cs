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
    }
}
