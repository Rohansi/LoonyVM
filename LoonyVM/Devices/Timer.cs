using System;
using System.Diagnostics;
using System.Threading;

namespace LoonyVM.Devices
{
    public class Timer : IDevice, IDisposable
    {
        public byte Id { get { return 0x01; } }

        private Thread _thread;
        private bool _running;

        private Stopwatch _watch;
        private bool _enabled;
        private double _frequency;
        private bool _interrupt;

        public Timer()
        {
            _watch = Stopwatch.StartNew();
            _enabled = false;
            _frequency = 1.0 / 100;
            _interrupt = false;

            _running = true;
            _thread = new Thread(() =>
            {
                while (_running)
                {
                    if (_enabled && _watch.Elapsed.TotalSeconds >= _frequency)
                    {
                        _interrupt = true;
                        _watch.Restart();
                    }

                    Thread.Sleep(1);
                }
            });

            _thread.Start();
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
            switch (machine.Registers[0])
            {
                case 0: // enable
                    _enabled = machine.Registers[1] != 0;
                    break;
                case 1: // set frequency
                    _frequency = 1.0 / Util.Clamp(machine.Registers[1], 1, 1000);
                    break;
            }
        }

        public void Dispose()
        {
            _running = false;
            _thread.Join();
        }
    }
}
