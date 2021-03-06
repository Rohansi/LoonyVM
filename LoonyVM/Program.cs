﻿using System.IO;
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

            var kbd = new Devices.Keyboard(0x02, Window);
            Machine.Attach(kbd);

            var display = new Devices.Display(0x06, Machine, Window);
            Machine.Attach(display);

            var hdd = new Devices.HardDrive(0x08, "disk.img");
            Machine.Attach(hdd);

            var running = true;

            var stepThread = new Thread(() =>
            {
                while (running)
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

            running = false;
            stepThread.Join();
            Machine.Dispose();
        }
    }
}
