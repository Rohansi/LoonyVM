using System.Diagnostics;
using SFML.Graphics;
using SFML.Window;
using Texter;

namespace LoonyVM.Devices
{
    public class Display : Drawable, IDevice
    {
        public enum VideoMode
        {
            Text = 0,
            Graphics = 1
        }

        public byte Id { get { return 0x06; } }

        private VideoMode _mode;
        private VirtualMachine _machine;
        private RenderWindow _window;

        private TextDisplay _textDisplay;
        private GraphicsDisplay _graphicsDisplay;

        private bool _cursorEnabled;
        private bool _cursorVisible;
        private Stopwatch _cursorTimer;
        private RectangleShape _cursor;

        public Display(VirtualMachine machine, RenderWindow window)
        {
            _machine = machine;
            _window = window;

            _textDisplay = new TextDisplay(80, 25);

            _graphicsDisplay = new GraphicsDisplay(320, 200);
            _graphicsDisplay.Scale = new Vector2f(2, 2);

            _cursorEnabled = false;
            _cursorVisible = false;
            _cursorTimer = Stopwatch.StartNew();

            var cursorSize = new Vector2f(_textDisplay.CharacterWidth, _textDisplay.CharacterHeight * 0.15f);
            _cursor = new RectangleShape(cursorSize);
            _cursor.Origin = new Vector2f(0, -(_textDisplay.CharacterHeight * 0.85f));
            _cursor.FillColor = _textDisplay.PaletteGet(15);

            ChangeVideoMode(VideoMode.Text);
        }

        public bool InterruptRequest
        {
            get { return false; }
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {

        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[(int)Register.R0])
            {
                case 0: // change mode
                    ChangeVideoMode((VideoMode)machine.Registers[(int)Register.R1]);
                    break;
                case 1: // enable cursor
                    _cursorEnabled = machine.Registers[(int)Register.R1] != 0;
                    break;
                case 2: // move cursor
                    _cursor.Position = new Vector2f(machine.Registers[(int)Register.R1] * _textDisplay.CharacterWidth,
                                                    machine.Registers[(int)Register.R2] * _textDisplay.CharacterHeight);
                    break;
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            var addr = 0x60000;

            switch (_mode)
            {
                case VideoMode.Text:
                    {
                        for (var y = 0; y < _textDisplay.Height; y++)
                        {
                            for (var x = 0; x < _textDisplay.Width; x++)
                            {
                                var glyph = (char)_machine.Memory[addr++];
                                var col = _machine.Memory[addr++];
                                var colF = col & 0x0F;
                                var colB = col >> 4;

                                _textDisplay.Set(x, y, new Character(glyph, colF, colB), false);
                            }
                        }

                        target.Draw(_textDisplay);

                        if (_cursorTimer.Elapsed.TotalSeconds > (1.0 / 2))
                        {
                            _cursorVisible = !_cursorVisible;
                            _cursorTimer.Restart();
                        }

                        if (_cursorEnabled && _cursorVisible)
                        {
                            target.Draw(_cursor);
                        }

                        break;
                    }

                case VideoMode.Graphics:
                    {
                        for (var y = 0; y < _graphicsDisplay.Height; y++)
                        {
                            for (var x = 0; x < _graphicsDisplay.Width; x++)
                            {
                                _graphicsDisplay.Set(x, y, _machine.Memory[addr++]);
                            }
                        }

                        target.Draw(_graphicsDisplay);
                        break;
                    }
            }
        }

        private void ChangeVideoMode(VideoMode mode)
        {
            switch (mode)
            {
                case VideoMode.Text:
                    _window.Size = new Vector2u(_textDisplay.Width * _textDisplay.CharacterWidth,
                                                _textDisplay.Height * _textDisplay.CharacterHeight);
                    break;
                case VideoMode.Graphics:
                    _window.Size = new Vector2u(_graphicsDisplay.Width * (uint)_graphicsDisplay.Scale.X,
                                                _graphicsDisplay.Height * (uint)_graphicsDisplay.Scale.Y);
                    break;
                default:
                    return;
            }

            _mode = mode;
        }
    }
}
