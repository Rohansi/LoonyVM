namespace LoonyVM.Devices
{
    public class SysCall : IDevice
    {
        public byte Id { get { return 0x0F; } }
        public bool InterruptRequest { get { return false; } }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            // calls IRQ on request
            machine.Interrupt(Id);
        }
    }
}
