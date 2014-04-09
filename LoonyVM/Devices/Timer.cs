using System;
using ThreadTimer = System.Threading.Timer;

namespace LoonyVM.Devices
{
    public class Timer : IDevice, IDisposable
    {
        public byte Id { get { return 0x01; } }

        private ThreadTimer _timer;
        private bool _enabled;
        private TimeSpan _frequency;
        private bool _interrupt;

        public Timer()
        {
            _enabled = false;
            _frequency = TimeSpan.FromSeconds(1.0 / 100);
            _interrupt = false;

            _timer = new ThreadTimer(o => _interrupt = true);
            _timer.Change(_frequency, _frequency);
        }

        public bool InterruptRequest
        {
            get { return _enabled && _interrupt; }
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            _interrupt = false;
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[(int)Register.R0])
            {
                case 0: // enable
                    _enabled = machine.Registers[(int)Register.R1] != 0;
                    _interrupt = false;
                    break;
                case 1: // set frequency
                    _frequency = TimeSpan.FromSeconds(1.0 / Util.Clamp(machine.Registers[(int)Register.R1], 1, 1000));
                    _timer.Change(_frequency, _frequency);
                    break;
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
