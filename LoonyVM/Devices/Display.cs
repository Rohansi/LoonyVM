using SFML.Graphics;
using SFML.Window;
using Texter;

namespace LoonyVM.Devices
{
    public class Display : Drawable, IDevice
    {
        public enum VideoMode
        {
            None = 0,
            Text = 1,
            Graphics = 2
        }

        public byte Id { get { return 0x06; } }

        private VirtualMachine _machine;
        private TextDisplay _textDisplay;

        public Display(VirtualMachine machine, RenderWindow window)
        {
            _machine = machine;
            _textDisplay = new TextDisplay(80, 25);
            window.Size = new Vector2u(_textDisplay.Width * _textDisplay.CharacterWidth,
                                       _textDisplay.Height * _textDisplay.CharacterHeight);
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
            
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            var addr = 0x60000;

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

            _textDisplay.Draw(target, new Vector2f());
        }
    }
}
