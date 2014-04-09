using System;
using System.IO;

namespace LoonyVM.Devices
{
    public class HardDrive : IDevice
    {
        public byte Id { get { return 0x08; } }

        public bool InterruptRequest { get { return false; } }

        private int _sectorSize;
        private FileStream _stream;
        private byte[] _sectorBuff;
        private int _sectorCount;

        public HardDrive(string filename, int sectorSize = 512)
        {
            _sectorSize = sectorSize;
            _stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            _sectorBuff = new byte[sectorSize];
            _sectorCount = (int)_stream.Length / sectorSize;
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[(int)Register.R0])
            {
                case 0: // specs
                    machine.Registers[(int)Register.R0] = _sectorSize;
                    machine.Registers[(int)Register.R1] = _sectorCount;
                    break;
                case 1: // read sector
                    ReadSector(machine, machine.Registers[(int)Register.R1], machine.Registers[(int)Register.R2]);
                    break;
                case 2: // write sector
                    WriteSector(machine, machine.Registers[(int)Register.R1], machine.Registers[(int)Register.R2]);
                    break;
                case 3: // read sectors
                    while (machine.Registers[(int)Register.R3]-- > 0)
                    {
                        ReadSector(machine, machine.Registers[(int)Register.R1], machine.Registers[(int)Register.R2]);
                        machine.Registers[(int)Register.R1] += _sectorSize;
                        machine.Registers[(int)Register.R2]++;
                    }
                    break;
                case 4: // write sectors
                    while (machine.Registers[(int)Register.R3]-- > 0)
                    {
                        WriteSector(machine, machine.Registers[(int)Register.R1], machine.Registers[(int)Register.R2]);
                        machine.Registers[(int)Register.R1] += _sectorSize;
                        machine.Registers[(int)Register.R2]++;
                    }
                    break;
                case 100: // identify
                    machine.Registers[(int)Register.R0] = 0xB10C;
                    break;
            }
        }

        private void ReadSector(VirtualMachine machine, int destination, int sector)
        {
            if (sector < 0 || sector > _sectorCount)
                throw new Exception("Sector out of range");

            _stream.Seek(sector * _sectorSize, SeekOrigin.Begin);

            if (_stream.Read(_sectorBuff, 0, _sectorSize) != _sectorSize)
                throw new Exception("Not enough data in sector (bad image?)");

            for (var i = 0; i < _sectorSize; i++, destination++)
            {
                machine.Memory[destination] = _sectorBuff[i];
            }
        }

        private void WriteSector(VirtualMachine machine, int source, int sector)
        {
            if (sector < 0 || sector > _sectorCount)
                throw new Exception("Sector out of range");

            for (var i = 0; i < _sectorSize; i++, source++)
            {
                _sectorBuff[i] = machine.Memory[source];
            }

            _stream.Seek(sector * _sectorSize, SeekOrigin.Begin);
            _stream.Write(_sectorBuff, 0, _sectorSize);
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {

        }
    }
}
