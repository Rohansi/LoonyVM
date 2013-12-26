using System.Collections.Generic;
using SFML.Graphics;
using SFMLKey = SFML.Window.Keyboard.Key;

namespace LoonyVM.Devices
{
    public class Keyboard : IDevice
    {
        private struct KeyEvent
        {
            public readonly bool Pressed;
            public readonly short Key;

            public KeyEvent(bool pressed, short key)
            {
                Pressed = pressed;
                Key = key;
            }
        }

        public byte Id { get { return 0x02; } }

        private bool _enabled;
        private Queue<KeyEvent> _eventQueue;

        public Keyboard(RenderWindow window)
        {
            _enabled = false;
            _eventQueue = new Queue<KeyEvent>(10);

            window.KeyPressed += (sender, args) =>
            {
                if (!_enabled || args.Code == SFMLKey.Unknown)
                    return;

                lock (_eventQueue)
                    _eventQueue.Enqueue(new KeyEvent(true, KeyToCode(args.Code)));
            };

            window.KeyReleased += (sender, args) =>
            {
                if (!_enabled || args.Code == SFMLKey.Unknown)
                    return;

                lock (_eventQueue)
                    _eventQueue.Enqueue(new KeyEvent(false, KeyToCode(args.Code)));
            };
        }

        public bool InterruptRequest
        {
            get { return _enabled && _eventQueue.Count > 0; }
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            lock (_eventQueue)
            {
                var ev = _eventQueue.Dequeue();
                machine.Registers[0] = ev.Pressed ? 1 : 0;
                machine.Registers[1] = ev.Key;
            }
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[0])
            {
                case 0: // enable
                    _enabled = machine.Registers[1] != 0;
                    break;
            }
        }

        private short KeyToCode(SFMLKey key)
        {
            short k;
            if (!KeyMap.TryGetValue(key, out k))
                return short.MaxValue;
            return k;
        }

        private static readonly Dictionary<SFMLKey, short> KeyMap;

        static Keyboard()
        {
            KeyMap = new Dictionary<SFMLKey, short>
            {
                { SFMLKey.Back, 8 },
                { SFMLKey.Tab, 9 },
                { SFMLKey.Return, 10 },
                { SFMLKey.Space, 32 },

                { SFMLKey.Up, 24 },
                { SFMLKey.Down, 25 },
                { SFMLKey.Right, 26 },
                { SFMLKey.Left, 27 },

                { SFMLKey.Num0, 48 },
                { SFMLKey.Num1, 49 },
                { SFMLKey.Num2, 50 },
                { SFMLKey.Num3, 51 },
                { SFMLKey.Num4, 52 },
                { SFMLKey.Num5, 53 },
                { SFMLKey.Num6, 54 },
                { SFMLKey.Num7, 55 },
                { SFMLKey.Num8, 56 },
                { SFMLKey.Num9, 57 },

                { SFMLKey.A, 97 },
                { SFMLKey.B, 98 },
                { SFMLKey.C, 99 },
                { SFMLKey.D, 100 },
                { SFMLKey.E, 101 },
                { SFMLKey.F, 102 },
                { SFMLKey.G, 103 },
                { SFMLKey.H, 104 },
                { SFMLKey.I, 105 },
                { SFMLKey.J, 106 },
                { SFMLKey.K, 107 },
                { SFMLKey.L, 108 },
                { SFMLKey.M, 109 },
                { SFMLKey.N, 110 },
                { SFMLKey.O, 111 },
                { SFMLKey.P, 112 },
                { SFMLKey.Q, 113 },
                { SFMLKey.R, 114 },
                { SFMLKey.S, 115 },
                { SFMLKey.T, 116 },
                { SFMLKey.U, 117 },
                { SFMLKey.V, 118 },
                { SFMLKey.W, 119 },
                { SFMLKey.X, 120 },
                { SFMLKey.Y, 121 },
                { SFMLKey.Z, 122 },

                { SFMLKey.LBracket, 91 },
                { SFMLKey.RBracket, 93 },
                { SFMLKey.SemiColon, 59 },
                { SFMLKey.Comma, 44 },
                { SFMLKey.Period, 46 },
                { SFMLKey.Quote, 39 },
                { SFMLKey.Slash, 47 },
                { SFMLKey.BackSlash, 92 },
                { SFMLKey.Tilde, 96 },
                { SFMLKey.Equal, 61 },
                { SFMLKey.Dash, 45 },
                
                // non-printable/special
                { SFMLKey.Escape, 128 },
                { SFMLKey.LControl, 129 },
                { SFMLKey.LShift, 130 },
                { SFMLKey.LAlt, 131 },
                { SFMLKey.LSystem, 132 },
                { SFMLKey.RControl, 133 },
                { SFMLKey.RShift, 134 },
                { SFMLKey.RAlt, 135 },
                { SFMLKey.RSystem, 136 },
                { SFMLKey.Menu, 137 },
                { SFMLKey.PageUp, 138 },
                { SFMLKey.PageDown, 139 },
                { SFMLKey.End, 140 },
                { SFMLKey.Home, 141 },
                { SFMLKey.Insert, 142 },
                { SFMLKey.Delete, 143 },
                { SFMLKey.Pause, 144 },

                { SFMLKey.F1, 145 },
                { SFMLKey.F2, 146 },
                { SFMLKey.F3, 147 },
                { SFMLKey.F4, 148 },
                { SFMLKey.F5, 149 },
                { SFMLKey.F6, 150 },
                { SFMLKey.F7, 151 },
                { SFMLKey.F8, 152 },
                { SFMLKey.F9, 153 },
                { SFMLKey.F10, 154 },
                { SFMLKey.F11, 155 },
                { SFMLKey.F12, 156 },
                { SFMLKey.F13, 157 },
                { SFMLKey.F14, 158 },
                { SFMLKey.F15, 159 },
                
                { SFMLKey.Numpad0, 160 },
                { SFMLKey.Numpad1, 161 },
                { SFMLKey.Numpad2, 162 },
                { SFMLKey.Numpad3, 163 },
                { SFMLKey.Numpad4, 164 },
                { SFMLKey.Numpad5, 165 },
                { SFMLKey.Numpad6, 166 },
                { SFMLKey.Numpad7, 167 },
                { SFMLKey.Numpad8, 168 },
                { SFMLKey.Numpad9, 169 },
                { SFMLKey.Add, 170 },
                { SFMLKey.Subtract, 171 },
                { SFMLKey.Multiply, 172 },
                { SFMLKey.Divide, 173 },
            };
        }
    }
}
