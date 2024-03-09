using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Sys = Cosmos.System;

namespace CosmosTTF.TestKernel {
    public class Kernel : Sys.Kernel {
        private Canvas canvas;
        private CGSSurface surface;

        private TTFFont noto;

        private int frames = 0;
        private int framesThisSecond = 0;
        private int currentSecond = 0;

        protected override void BeforeRun() {
            VFSManager.RegisterVFS(new CosmosVFS());

            canvas = FullScreenCanvas.GetFullScreenCanvas();
            surface = new CGSSurface(canvas);

            noto = new TTFFont(File.ReadAllBytes(@"1:\noto.ttf"));
        }

        protected override void Run() {
            canvas.Clear();
            noto.DrawToSurface(surface, 48, 100, 100, "Hello World, we have " + framesThisSecond + " FPS!", Color.White);
            noto.DrawToSurface(surface, 24, 200, 200, "Hello World, we have " + framesThisSecond + " FPS!", Color.White);

            canvas.Display();
            frames++;

            if(RTC.Second != currentSecond) {
                currentSecond = RTC.Second;
                framesThisSecond = frames;
                frames = 0;
            }

            if (frames % 50 == 0) Heap.Collect();
        }
    }
}
