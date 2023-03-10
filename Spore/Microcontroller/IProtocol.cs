using static Microcontroller.Config;

namespace Microcontroller
{
    internal interface IProtocol : IDisposable
    {
        internal enum MainCommand
        {
            Get,
            Set,
        }

        internal enum SubCommand
        {
            Analog,
            Digital,
        }

        internal enum PinMap
        {
            A0 = 60,
            A1, A2, A3, A4, A5, A6, A7,
            A8, A9, A10, A11, A12, A13,
            A14, A15,
        }

        /// <summary>
        /// Responsible for extracting and publishing messages received from the Arduino
        /// </summary>
        public Publisher Publisher { get; set; }
        /// <summary>
        /// Will terminate the publisher when the instance is disposed
        /// </summary>
        public CancellationTokenSource CTS { get; set; }

        /// <summary>
        /// Create the instance and start the publisher
        /// </summary>
        /// <returns></returns>
        public bool Start();
        /// <summary>
        /// Send set commands to the Arduino
        /// </summary>
        /// <param name="subCmd"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public void Set(SubCommand subCmd, int pin = 0, int value = 0);
        /// <summary>
        /// Ask the Arduino for a response packet
        /// </summary>
        /// <param name="data">
        /// An array describing the types of pins requiring input readings
        /// </param>
        public void Get(byte[] data);
    }
}
