using Maestro;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length == 1) Microcontroller.Config.UDP_ADDR = args[0]; // Parse args, if any
        Logger.Initialize(); // Init Serilog

        // Allocate memory for the real-time image
        int colorWid = Camera.Config.MAX_WID;
        int colorHgt = Camera.Config.MAX_HGT;
        Color[] Colors = new Color[colorWid * colorHgt]; // Pointer to the first element is used by Window

        // Start the RayLib thread
        Thread window = new(() =>
        {
            unsafe
            {
                fixed (Color* colorPtr = &Colors[0])
                {
                    Window.UI.Main(colorPtr, colorWid, colorHgt);
                    Console.WriteLine("*Exiting... have a good day!*");
                }
            }
        });
        window.Start();

        // Init and poll camera's processed image
        Camera.ICamera camera = new Camera.USB2Cam();
        Thread cameraThread = new(() =>
        {
            camera.Start();
            while (window.ThreadState != ThreadState.Stopped)
            {
                Array.Copy(camera.ProcessedColors, Colors, camera.ProcessedColors.Length);
            }
        });
        cameraThread.Start();

        // Poll the console window
        while (window.ThreadState != ThreadState.Stopped)
        {
            try
            {
                string? s = Reader.ReadLine(5000);
                if (s != null)
                {
                    switch (s.ToLower())
                    {
                        case "clear":
                            Console.Clear();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (TimeoutException) { }
        }
    }
}