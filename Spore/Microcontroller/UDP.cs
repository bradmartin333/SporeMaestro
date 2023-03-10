using static Microcontroller.Config.Packet;
using static Microcontroller.Config;
using static Microcontroller.IProtocol;
using Serilog;
using System.Net.Sockets;
using System.Net;

namespace Microcontroller
{
    internal class UDP : IDisposable, IProtocol
    {
        public Publisher Publisher { get; set; } = new();
        public CancellationTokenSource CTS { get; set; } = new();

        private readonly IPEndPoint? IPEP;
        private readonly Socket? Socket;

        private bool NoUDPConnection = false; // Prevent spamming of the log when a device is not connected

        internal UDP(string ip, int port)
        {
            try
            {
                IPEP = new IPEndPoint(IPAddress.Parse(ip), port);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Log.Verbose("UDP constructed");
            }
            catch (Exception ex)
            {
                Log.Error($"UDP constructor exception: {ex.Message}");
            }
        }

        public bool Start()
        {
            try
            {
                if (Socket != null && IPEP != null && !Socket.Connected)
                    Socket.Connect(IPEP);

                UDPWorker worker = new(Publisher, Socket, CTS.Token); // Start RX worker

                Log.Verbose("UDP started");
            }
            catch (Exception ex)
            {
                Log.Error($"UDP start exception: {ex.Message}");
                return false;
            }
            return true;
        }

        public void Set(SubCommand subCmd, int pin = 0, int value = 0)
        {
            // UDP drops packets by design, so lets be sure it gets there!
            for (int i = 0; i < 3; i++)
            {
                ErrorMessage udpError = ErrorMessage.None;
                Communicate(MainCommand.Set, new byte[] { (byte)subCmd }, pin, value, ref udpError);
                if (udpError != ErrorMessage.None) Log.Error($"UDP set error: {udpError}");
                Thread.Sleep(10); // One Arduino cycle
            }
            Log.Debug($"UDP: {subCmd} pin {pin} = {value}");
        }

        public void Get(byte[] data)
        {
            ErrorMessage udpError = ErrorMessage.None;
            Communicate(MainCommand.Get, data, 0, 0, ref udpError);
            if (udpError != ErrorMessage.None) Log.Error($"UDP get error: {udpError}");
        }

        private void Communicate(MainCommand cmd, byte[] subCmd, int pin, int value, ref ErrorMessage udpError)
        {
            byte[] tx = new byte[PACKET_SIZE];
            Array.Fill(tx, NULL_PLACEHOLDER);
            tx[0] = TX_START;
            tx[PACKET_SIZE - 1] = TX_END;

            switch (cmd)
            {
                case MainCommand.Get:
                    tx[1] = (byte)cmd;
                    Array.Copy(subCmd, 0, tx, 2, NUM_PIN_BYTES); // Copy the array describing which pins are inputs
                    break;
                case MainCommand.Set:
                    tx[1] = (byte)cmd;
                    tx[2] = subCmd[0];
                    byte[] pinData = BitConverter.GetBytes(pin); // Convert the passed value to bytes 3-6
                    byte[] valueData = BitConverter.GetBytes(value); // Convert the passed value to bytes 7-10
                    Array.Copy(pinData, 0, tx, 3, 4);
                    Array.Copy(valueData, 0, tx, 7, 4);
                    break;
                default:
                    break;
            }

            if (Socket == null)
                udpError = ErrorMessage.NoConnection;
            else
            {
                try
                {
                    Socket.Send(tx);
                    NoUDPConnection = false;
                }
                catch (Exception)
                {
                    if (!NoUDPConnection)
                    {
                        udpError = ErrorMessage.NoResponse;
                        NoUDPConnection = true;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Socket != null && Socket.Connected)
            {
                CTS.Cancel();
                Socket.Close();
                Log.Verbose("UDP disposed");
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class UDPWorker
    {
        private static readonly byte[] NullAnalogReading = Enumerable.Repeat(NULL_PLACEHOLDER, 4).ToArray();

        private static bool AnalogReadingIsNull(byte[] data, int index)
        {
            int length = 4;
            byte[] subArr = new byte[length];
            Array.Copy(data, index, subArr, 0, length);
            return Enumerable.SequenceEqual(subArr, NullAnalogReading);
        }

        internal UDPWorker(Publisher publisher, Socket? socket, CancellationToken token)
        {
            if (token.CanBeCanceled && socket != null)
            {
                new Thread(() =>
                {
                    Log.Verbose("UDPWorker constructed");
                    while (!token.IsCancellationRequested)
                    {
                        ErrorMessage udpError = ErrorMessage.None;

                        // Wait for packet
                        if (socket.Available < PACKET_SIZE)
                            continue;

                        try // Read packet
                        {
                            byte[] rx = new byte[PACKET_SIZE];
                            socket.Receive(rx, 0, PACKET_SIZE, SocketFlags.None);

                            // Check that data is valid
                            if (rx.First() != RX_START || rx.Last() != RX_END)
                                udpError = ErrorMessage.InvalidMarkers;

                            // Update info
                            if (udpError == ErrorMessage.None)
                            {
                                // Skip the start and end bits
                                for (int i = 2; i < PACKET_SIZE - 1; i++)
                                {
                                    int pin = i - 2; // Adjust for start and cmd bytes
                                    if (i < (int)PinMap.A0 + 2) // Digital read
                                    {
                                        if (rx[i] == NULL_PLACEHOLDER)
                                            continue;
                                        else
                                            publisher.RaiseEvent(pin, rx[i]);
                                    }
                                    else if (i > (int)PinMap.A0 && pin % 4 == 0) // Analog read
                                    {
                                        // Possible to have 250 (NULL_PLACEHOLDER) in a position
                                        if (AnalogReadingIsNull(rx, i))
                                            continue;
                                        // Map the packet index to the pin
                                        int analogPin = (int)PinMap.A0 + (pin - (int)PinMap.A0) / 4;
                                        int value = BitConverter.ToInt32(rx, i);
                                        publisher.RaiseEvent(analogPin, value);
                                    }
                                }
                            }
                            else
                                Log.Error($"UDP worker error: {udpError}");
                        }
                        catch (Exception ex) // When closing via debugger
                        {
                            Log.Warning($"UDP still open: {ex.Message}");
                        }
                    }
                    Log.Verbose("UDPWorker disposed");
                }).Start();
            }
        }
    }
}
