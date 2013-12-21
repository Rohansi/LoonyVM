
namespace LoonyVM
{
    public partial class VirtualMachine : IDevice
    {
        public enum ExceptionCode
        {
            InvalidOpcode        = 0x00,
            DivideByZero         = 0x01,
            MemoryBounds         = 0x02,

            Breakpoint           = 0x10,
        }

        public byte Id { get { return 0x00; } }

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

        private void Exception(ExceptionCode code)
        {
            IP = _errorIp;
            Interrupt(Id);
            Registers[0] = (int)code;
        }
    }
}
