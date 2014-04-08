namespace LoonyVM
{
    public interface IDevice
    {
        /// <summary>
        /// Interrupt ID for the device
        /// </summary>
        byte Id { get; }

        /// <summary>
        /// Returns true when the device needs to interrupt the CPU.
        /// When available, HandleInterruptRequest will be called.
        /// </summary>
        bool InterruptRequest { get; }

        /// <summary>
        /// Handles an interrupt request (requested by the device, not program).
        /// </summary>
        void HandleInterruptRequest(VirtualMachine machine);

        /// <summary>
        /// Handles a request from the program.
        /// </summary>
        void HandleInterrupt(VirtualMachine machine);
    }
}
