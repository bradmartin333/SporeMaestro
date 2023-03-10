using Maestro;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length == 1) Microcontroller.Config.UDP_ADDR = args[0]; // Parse args, if any
        Logger.Initialize(); // Init Serilog
        
        // Start the RayLib thread
        Thread window = new(() =>
        {
            unsafe
            {
                Window.UI.Main();
                Console.WriteLine("*Exiting... have a good day!*");
            }
        });
        window.Start();

        // Poll the console window
        while (window.ThreadState != ThreadState.Stopped)
        {
            try
            {
                string? s = Reader.ReadLine(5000);
                if (s != null) Console.WriteLine($"I got {s}");
            }
            catch (TimeoutException) { }
        }
    }
}