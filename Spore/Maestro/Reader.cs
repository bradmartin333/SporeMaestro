namespace Maestro
{
    internal static class Reader
    {
        private static readonly Thread? InputThread;
        private static readonly AutoResetEvent? GetInput;
        private static readonly AutoResetEvent? GotInput;
        private static string? Input;

        static Reader()
        {
            GetInput = new AutoResetEvent(false);
            GotInput = new AutoResetEvent(false);
            InputThread = new Thread(Read)
            {
                IsBackground = true
            };
            InputThread.Start();
        }

        private static void Read()
        {
            if (GetInput == null || GotInput == null) return;
            while (true)
            {
                GetInput.WaitOne();
                Input = Console.ReadLine();
                GotInput.Set();
            }
        }

        public static string ReadLine(int timeOutMillisecs = Timeout.Infinite)
        {
            if (GetInput == null || GotInput == null) return string.Empty;
            GetInput.Set();
            bool success = GotInput.WaitOne(timeOutMillisecs);
            if (success)
                return Input ?? string.Empty;
            else
                throw new TimeoutException("User did not provide input within the timelimit.");
        }
    }
}
