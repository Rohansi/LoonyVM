using SFML.Graphics;

namespace LoonyVM
{
    public class GraphicsDisplay : Transformable, Drawable
    {
        public readonly uint Width;
        public readonly uint Height;

        private Image _data;
        private Texture _dataTexture;
        private Image _palette;
        private Texture _paletteTexture;
        private Sprite _display;
        private Shader _renderer;

        public GraphicsDisplay(uint width, uint height)
        {
            Width = width;
            Height = height;

            _data = new Image(width, height, Color.Black);
            _dataTexture = new Texture(_data);
            _palette = new Image("Data/palette.png");
            _paletteTexture = new Texture(_palette);

            _display = new Sprite(_dataTexture);

            _renderer = new Shader("Data/display.vert", "Data/display.frag");
            _renderer.SetParameter("data", _dataTexture);
            _renderer.SetParameter("dataSize", width, height);
            _renderer.SetParameter("palette", _paletteTexture);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _paletteTexture.Update(_palette);
            _dataTexture.Update(_data);

            states.Transform *= Transform;
            states.Shader = _renderer;

            target.Draw(_display, states);
        }

        public void Set(int x, int y, byte col)
        {
            if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1)
                return;

            _data.SetPixel((uint)x, (uint)y, new Color(col, 0, 0));
        }
    }
}
