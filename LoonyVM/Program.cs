using System.IO;
using System.Threading;

namespace LoonyVM
{
    class Program
    {
        static void Main(string[] args)
        {
            var vm = new VirtualMachine();
            var prog = File.ReadAllBytes("test.bin");

            for (var i = 0; i < prog.Length; i++)
                vm.Memory[i] = prog[i];

            while (true)
            {
                vm.Step();
                //Thread.Sleep(250);
            }
        }
    }
}
