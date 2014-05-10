using System.Collections.Generic;

namespace LoonyVM.Devices
{
    public class Serial : IDevice
    {
        public byte Id { get; private set; }

        private bool _enabled;
        private Queue<byte> _inputQueue;
        private Queue<byte> _outputQueue;

        public Serial(byte id)
        {
            Id = id;

            _enabled = false;
            _inputQueue = new Queue<byte>();
            _outputQueue = new Queue<byte>();
        }

        public bool InterruptRequest
        {
            get
            {
                lock (_inputQueue)
                    return _enabled && _inputQueue.Count > 0;
            }
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            lock (_inputQueue)
            {
                var value = _inputQueue.Dequeue();
                machine.Registers[(int)Register.R0] = value;
            }
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[(int)Register.R0])
            {
                case 0: // enable
                    _enabled = machine.Registers[(int)Register.R1] != 0;
                    break;
                case 1: // output
                    lock (_outputQueue)
                        _outputQueue.Enqueue((byte)(machine.Registers[(int)Register.R1] & 0xFF)); // TODO: this should have a cooldown
                    break;
            }
        }

        public bool TryRead(out byte value)
        {
            lock (_outputQueue)
            {
                if (_outputQueue.Count == 0)
                {
                    value = 0;
                    return false;
                }

                value = _outputQueue.Dequeue();
                return true;
            }
        }

        public void Write(byte value)
        {
            lock (_inputQueue)
                _inputQueue.Enqueue(value);
        }
    }
}
