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

        // Poll the console window
        while (window.ThreadState != ThreadState.Stopped)
        {
            try
            {
                string? s = Reader.ReadLine(5000);
                if (s != null)
                {
                    switch (s)
                    {
                        case "r":
                            Array.Fill(Colors, RED);
                            break;
                        case "g":
                            Array.Fill(Colors, GREEN);
                            break;
                        case "b":
                            Array.Fill(Colors, BLUE);
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