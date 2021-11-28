using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Engine
{
    public static class Input
    {
        private static KeyboardState _keyboardState;
        
        public static void Initialize(Game game)
        {
            game.OnInput += UpdateKeyboardState;
        }

        private static void UpdateKeyboardState(InputEventArgs args)
        {
            _keyboardState = args.State;
        }

        public static bool IsKeyDown(Keys keys)
        {
            return _keyboardState != null && _keyboardState.IsKeyDown(keys);
        }
        public static bool IsKeyReleased(Keys keys)
        {
            return _keyboardState != null && _keyboardState.IsKeyReleased(keys);
        }
        public static bool IsAnyKeyDown()
        {
            return _keyboardState != null && _keyboardState.IsAnyKeyDown;
        }
    }

    public class InputEventArgs
    {
        public readonly KeyboardState State;

        public InputEventArgs(KeyboardState state)
        {
            State = state;
        }
    }
}
