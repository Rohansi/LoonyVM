using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoonyVM
{
    public partial class VirtualMachine : IDevice
    {
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
    }
}
