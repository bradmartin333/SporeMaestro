namespace Microcontroller
{
    public class Config
    {
        public static string? UDP_ADDR { get; set; } = null;

        internal static class Packet
        {
            internal enum ErrorMessage
            {
                None,
                NoConnection,
                NoResponse,
                InvalidMarkers,
                InvalidResponse
            }

            internal const int NUM_PINS = 76; // 76 pins total
                                              // 1 (byte) * 60 digital + 4 (int) * 16 analog = 124
            internal const int NUM_PIN_BYTES = 124;
            // 124 + start (byte) + end (byte) + command (byte) = 127
            internal const int PACKET_SIZE = NUM_PIN_BYTES + 3;
            internal const byte TX_START = 254;
            internal const byte TX_END = 251;
            internal const byte RX_START = 253;
            internal const byte RX_END = 252;
            internal const byte NULL_PLACEHOLDER = 250;
        }

        internal class Publisher
        {
            internal delegate void PacketReceivedEventHandler(object sender, PacketReceivedArgs e);
            internal event PacketReceivedEventHandler? PacketReceived;

            internal void RaiseEvent(int pin, int value)
            {
                PacketReceived?.Invoke(this, new PacketReceivedArgs() { Pin = pin, Value = value });
            }
        }

        internal class PacketReceivedArgs : EventArgs
        {
            internal int Pin { get; set; }
            internal int Value { get; set; }
        }

    }
}