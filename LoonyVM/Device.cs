
namespace LoonyVM
{
    public abstract class Device
    {
        /// <summary>
        /// Interrupt ID for the device
        /// </summary>
        public abstract byte Id { get; }

        /// <summary>
        /// Returns true when the device needs to interrupt the CPU.
        /// When available, HandleInterruptRequest will be called.
        /// </summary>
        public abstract bool InterruptRequest { get; }

        /// <summary>
        /// Handles an interrupt request (requested by the device, not program).
        /// </summary>
        public abstract void HandleInterruptRequest(VirtualMachine machine);

        /// <summary>
        /// Handles a request from the program.
        /// </summary>
        public abstract void HandleInterrupt(VirtualMachine machine);
    }
}
