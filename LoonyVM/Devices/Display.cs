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

        public Display(VirtualMachine machine, RenderWindow window)
        {
            _machine = machine;
            _window = window;

            _textDisplay = new TextDisplay(80, 25);

            _graphicsDisplay = new GraphicsDisplay(320, 200);
            _graphicsDisplay.Scale = new Vector2f(2, 2);

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
            switch (machine.Registers[0])
            {
                case 0:
                    ChangeVideoMode((VideoMode)machine.Registers[1]);
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

                    _textDisplay.Draw(target);
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

                    _graphicsDisplay.Draw(target);
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
