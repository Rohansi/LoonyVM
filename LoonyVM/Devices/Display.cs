using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoonyVM.Devices
{
    public class Display : Device
    {
        public override byte Id
        {
            get { return 0x08; }
        }

        public override bool InterruptRequest
        {
            get { return false; }
        }

        public override void HandleInterruptRequest(VirtualMachine machine)
        {
            
        }

        public override void HandleInterrupt(VirtualMachine machine)
        {
            
        }
    }
}
