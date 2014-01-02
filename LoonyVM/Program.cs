using System.IO;
using System.Threading;
using SFML.Graphics;
using SFML.Window;

namespace LoonyVM
{
    public static class Program
    {
        public static VirtualMachine Machine;
        public static RenderWindow Window;

        public static void Main(string[] args)
        {
            Window = new RenderWindow(new VideoMode(640, 480), "", Styles.Close);
            Window.SetFramerateLimit(60);

            Window.Closed += (sender, eventArgs) => Window.Close();

            Window.Resized += (sender, eventArgs) =>
            {
                var view = new View();
                view.Size = new Vector2f(eventArgs.Width, eventArgs.Height);
                view.Center = view.Size / 2;
                Window.SetView(view);
            };

            Machine = new VirtualMachine(512 * 1024);

            var prog = File.ReadAllBytes("bios.bin");
            for (var i = 0; i < prog.Length; i++)
                Machine.Memory[i] = prog[i];

            var display = new Devices.Display(Machine, Window);
            Machine.Attach(display);

            var timer = new Devices.Timer();
            Machine.Attach(timer);

            var kbd = new Devices.Keyboard(Window);
            Machine.Attach(kbd);

            var hdd = new Devices.HardDrive("disk.img");
            Machine.Attach(hdd);

            var stepThread = new Thread(() =>
            {
                while (Window.IsOpen())
                {
                    Machine.Step();
                }
            });

            stepThread.Start();

            while (Window.IsOpen())
            {
                Window.DispatchEvents();

                Window.Clear();
                Window.Draw(display);
                Window.Display();
            }

            timer.Dispose();
        }
    }
}
