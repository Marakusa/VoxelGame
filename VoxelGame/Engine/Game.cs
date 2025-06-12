using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Game;

namespace VoxelGame.Engine
{
    public class Game : GameWindow
    {
        public Game(int width, int height, string title)
            : base(new(),
                new()
                {
                    Size = new Vector2i(width, height),
                    Title = title,
                    APIVersion = new(4, 6),
                    API = ContextAPI.OpenGL,
                    NumberOfSamples = 8,
                })
        { }

        private Renderer _graphicsRenderer;

        private Player _player;
        private Camera _playerCamera;

        public bool CursorVisible { get; private set; }

        protected override void OnLoad()
        {
            Noise.SetNoise(1f, 1f);
            Blocks.Initialize();

            Input.Initialize(this);
            
            _graphicsRenderer = new();

            _playerCamera = new(new Vector3(8f, 65f, 8f));
            _player = new(_playerCamera, _graphicsRenderer.Highlight);

            _graphicsRenderer.Load(_player, _playerCamera);
            
            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _graphicsRenderer.RenderFrame(this);

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnUnload()
        {
            CursorVisible = true;
            ChunkManager.DeleteChunkBuffers();
            _graphicsRenderer.Unload();
            if (File.Exists("assets/atlas.png")) File.Delete("assets/atlas.png");
            base.OnUnload();
        }

        public delegate void InputEventHandler(InputEventArgs args);
        public event InputEventHandler OnInput;
        
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!IsFocused) return;

            OnInput?.Invoke(new(KeyboardState.GetSnapshot()));

            _playerCamera.Movement(e);
            _playerCamera.Update();

            Vector2 currentChunk = new(_playerCamera.Position.X / ChunkManager.GetWidth(), _playerCamera.Position.Z / ChunkManager.GetWidth());

            if (currentChunk != _playerCamera.LastChunk)
            {
                _playerCamera.LastChunk = currentChunk;
                ChunkManager.LoadChunks(_playerCamera.Position);
            }

            CursorVisible = !_playerCamera.IsLocked;

            if (Input.IsKeyDown(Keys.D1))
                _player?.SetCurrentSlot(0);
            else if (Input.IsKeyDown(Keys.D2))
                _player?.SetCurrentSlot(1);
            else if (Input.IsKeyDown(Keys.D3))
                _player?.SetCurrentSlot(2);
            else if (Input.IsKeyDown(Keys.D4))
                _player?.SetCurrentSlot(3);
            else if (Input.IsKeyDown(Keys.D5))
                _player?.SetCurrentSlot(4);
            else if (Input.IsKeyDown(Keys.D6))
                _player?.SetCurrentSlot(5);
            else if (Input.IsKeyDown(Keys.D7))
                _player?.SetCurrentSlot(6);
            else if (Input.IsKeyDown(Keys.D8))
                _player?.SetCurrentSlot(7);
            else if (Input.IsKeyDown(Keys.D9))
                _player?.SetCurrentSlot(8);

            if (Input.IsKeyDown(Keys.Escape))
                Close();
            
            _player?.Update();

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (_playerCamera.IsLocked)
            {
                _playerCamera.Look(MousePosition);
                MousePosition = new(Size.X / 2f, Size.Y / 2f);
                _playerCamera.UpdateMouseLock(new Vector2(Size.X / 2f, Size.Y / 2f));
            }

            base.OnMouseMove(e);
        }

        private bool _mouseDown;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (_playerCamera.IsLocked && !_mouseDown)
            {
                _player.MouseDown(e);
                _mouseDown = true;
            }
            
            _playerCamera.IsLocked = true;
            
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _mouseDown = false;
            
            base.OnMouseUp(e);
        }
    }
}