using System;

namespace LoonyVM
{
    public class VirtualMachineException : Exception
    {
        public VirtualMachineException(int ip, string message)
            : base(string.Format("{0:X8}: {1}", ip, message))
        {
            
        }
    }

    internal class VirtualMachineInvalidOpcode : VirtualMachineException
    {
        public VirtualMachineInvalidOpcode(string message)
            : base(0, message)
        {

        }
    }
}
