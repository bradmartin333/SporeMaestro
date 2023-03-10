using static Microcontroller.Config;
using static Microcontroller.Config.Packet;
using static Microcontroller.IProtocol;
using Serilog;

namespace Microcontroller
{
    internal class Board : IDisposable
    {
        internal enum BoardTypes
        {
            Uno,
            Mega,
            Nucleo,
            APMC,
        }

        internal Pin D0 { get; set; } = new(); // Only present on Nucleo
        internal Pin D1 { get; set; } = new(); // Only present on Nucleo
        internal Pin D2 { get; set; } = new();
        internal Pin D3 { get; set; } = new();
        internal Pin D4 { get; set; } = new();
        internal Pin D5 { get; set; } = new();
        internal Pin D6 { get; set; } = new();
        internal Pin D7 { get; set; } = new();
        internal Pin D8 { get; set; } = new();
        internal Pin D9 { get; set; } = new();
        internal Pin D10 { get; set; } = new(); // D10 - D13 Used by Ethernet shield
        internal Pin D11 { get; set; } = new();
        internal Pin D12 { get; set; } = new();
        internal Pin D13 { get; set; } = new();
        internal Pin D14 { get; set; } = new(); // Only present on Nucleo
        internal Pin D15 { get; set; } = new(); // Only present on Nucleo
        internal Pin D16 { get; set; } = new(); // D16 - D19 only for custom scenarios
        internal Pin D17 { get; set; } = new();
        internal Pin D18 { get; set; } = new();
        internal Pin D19 { get; set; } = new();
        internal Pin D20 { get; set; } = new(); // SDA on Mega
        internal Pin D21 { get; set; } = new(); // SCL on Mega
        internal Pin D22 { get; set; } = new();
        internal Pin D23 { get; set; } = new();
        internal Pin D24 { get; set; } = new();
        internal Pin D25 { get; set; } = new();
        internal Pin D26 { get; set; } = new();
        internal Pin D27 { get; set; } = new();
        internal Pin D28 { get; set; } = new();
        internal Pin D29 { get; set; } = new();
        internal Pin D30 { get; set; } = new();
        internal Pin D31 { get; set; } = new();
        internal Pin D32 { get; set; } = new();
        internal Pin D33 { get; set; } = new();
        internal Pin D34 { get; set; } = new();
        internal Pin D35 { get; set; } = new();
        internal Pin D36 { get; set; } = new();
        internal Pin D37 { get; set; } = new();
        internal Pin D38 { get; set; } = new();
        internal Pin D39 { get; set; } = new();
        internal Pin D40 { get; set; } = new();
        internal Pin D41 { get; set; } = new();
        internal Pin D42 { get; set; } = new();
        internal Pin D43 { get; set; } = new();
        internal Pin D44 { get; set; } = new();
        internal Pin D45 { get; set; } = new();
        internal Pin D46 { get; set; } = new();
        internal Pin D47 { get; set; } = new();
        internal Pin D48 { get; set; } = new();
        internal Pin D49 { get; set; } = new();
        internal Pin D50 { get; set; } = new();
        internal Pin D51 { get; set; } = new();
        internal Pin D52 { get; set; } = new();
        internal Pin D53 { get; set; } = new();
        internal Pin D54 { get; set; } = new(); // D54 - D59 only for custom scenarios
        internal Pin D55 { get; set; } = new();
        internal Pin D56 { get; set; } = new();
        internal Pin D57 { get; set; } = new();
        internal Pin D58 { get; set; } = new();
        internal Pin D59 { get; set; } = new();

        internal Pin A0 { get; set; } = new Pin((int)PinMap.A0, Pin.Configs.Analog);
        internal Pin A1 { get; set; } = new Pin((int)PinMap.A1, Pin.Configs.Analog);
        internal Pin A2 { get; set; } = new Pin((int)PinMap.A2, Pin.Configs.Analog);
        internal Pin A3 { get; set; } = new Pin((int)PinMap.A3, Pin.Configs.Analog);
        internal Pin A4 { get; set; } = new Pin((int)PinMap.A4, Pin.Configs.Analog);
        internal Pin A5 { get; set; } = new Pin((int)PinMap.A5, Pin.Configs.Analog);
        internal Pin A6 { get; set; } = new Pin((int)PinMap.A6, Pin.Configs.Analog);
        internal Pin A7 { get; set; } = new Pin((int)PinMap.A7, Pin.Configs.Analog);
        internal Pin A8 { get; set; } = new Pin((int)PinMap.A8, Pin.Configs.Analog);
        internal Pin A9 { get; set; } = new Pin((int)PinMap.A9, Pin.Configs.Analog);
        internal Pin A10 { get; set; } = new Pin((int)PinMap.A10, Pin.Configs.Analog);
        internal Pin A11 { get; set; } = new Pin((int)PinMap.A11, Pin.Configs.Analog);
        internal Pin A12 { get; set; } = new Pin((int)PinMap.A12, Pin.Configs.Analog);
        internal Pin A13 { get; set; } = new Pin((int)PinMap.A13, Pin.Configs.Analog);
        internal Pin A14 { get; set; } = new Pin((int)PinMap.A14, Pin.Configs.Analog);
        internal Pin A15 { get; set; } = new Pin((int)PinMap.A15, Pin.Configs.Analog);

        internal readonly Pin[] Pins = new Pin[NUM_PINS];
        private readonly IProtocol Protocol;

        internal Board(BoardTypes boardType, IProtocol protocol)
        {
            Protocol = protocol;
            Protocol.Start();
            Protocol.Publisher.PacketReceived += Publisher_PacketReceived;

            switch (boardType)
            {
                case BoardTypes.Uno:
                    D2 = new(2, Pin.Configs.Digital);
                    D3 = new(3, Pin.Configs.PWM);
                    D5 = new(5, Pin.Configs.PWM);
                    D6 = new(6, Pin.Configs.PWM);
                    D7 = new(7, Pin.Configs.Digital);
                    D8 = new(8, Pin.Configs.Digital);
                    D9 = new(9, Pin.Configs.PWM);
                    break;
                case BoardTypes.Mega:
                    D2 = new(2, Pin.Configs.PWM);
                    D3 = new(3, Pin.Configs.PWM);
                    D5 = new(5, Pin.Configs.PWM);
                    D6 = new(6, Pin.Configs.PWM);
                    D7 = new(7, Pin.Configs.PWM);
                    D8 = new(8, Pin.Configs.PWM);
                    D9 = new(9, Pin.Configs.PWM);
                    D20 = new(20, Pin.Configs.Digital);
                    D21 = new(20, Pin.Configs.Digital);
                    D22 = new(22, Pin.Configs.Digital);
                    D23 = new(23, Pin.Configs.Digital);
                    D24 = new(24, Pin.Configs.Digital);
                    D25 = new(25, Pin.Configs.Digital);
                    D26 = new(26, Pin.Configs.Digital);
                    D27 = new(27, Pin.Configs.Digital);
                    D28 = new(28, Pin.Configs.Digital);
                    D29 = new(29, Pin.Configs.Digital);
                    D30 = new(30, Pin.Configs.Digital);
                    D31 = new(31, Pin.Configs.Digital);
                    D32 = new(32, Pin.Configs.Digital);
                    D33 = new(33, Pin.Configs.Digital);
                    D34 = new(34, Pin.Configs.Digital);
                    D35 = new(35, Pin.Configs.Digital);
                    D36 = new(36, Pin.Configs.Digital);
                    D37 = new(37, Pin.Configs.Digital);
                    D38 = new(38, Pin.Configs.Digital);
                    D39 = new(39, Pin.Configs.Digital);
                    D40 = new(40, Pin.Configs.Digital);
                    D41 = new(41, Pin.Configs.Digital);
                    D42 = new(42, Pin.Configs.Digital);
                    D43 = new(43, Pin.Configs.Digital);
                    D44 = new(44, Pin.Configs.PWM);
                    D45 = new(45, Pin.Configs.PWM);
                    D46 = new(46, Pin.Configs.PWM);
                    D47 = new(47, Pin.Configs.Digital);
                    D48 = new(48, Pin.Configs.Digital);
                    D49 = new(49, Pin.Configs.Digital);
                    D50 = new(50, Pin.Configs.Digital);
                    D51 = new(51, Pin.Configs.Digital);
                    D52 = new(52, Pin.Configs.Digital);
                    D53 = new(53, Pin.Configs.Digital);
                    break;
                case BoardTypes.Nucleo:
                    D0 = new(0, Pin.Configs.Digital);
                    D1 = new(1, Pin.Configs.Digital);
                    D2 = new(2, Pin.Configs.Digital);
                    D3 = new(3, Pin.Configs.PWM);
                    D5 = new(5, Pin.Configs.PWM);
                    D6 = new(6, Pin.Configs.PWM);
                    D7 = new(7, Pin.Configs.Digital);
                    D8 = new(8, Pin.Configs.Digital);
                    D9 = new(9, Pin.Configs.PWM);
                    D14 = new(14, Pin.Configs.Digital);
                    D15 = new(15, Pin.Configs.Digital);
                    break;
                case BoardTypes.APMC:
                    // J3
                    D0 = new(0, Pin.Configs.Digital);
                    D1 = new(1, Pin.Configs.Digital);
                    D2 = new(2, Pin.Configs.Digital);
                    D3 = new(3, Pin.Configs.Digital);
                    D4 = new(4, Pin.Configs.Digital);
                    D5 = new(5, Pin.Configs.Digital);
                    D6 = new(6, Pin.Configs.Digital);
                    D7 = new(7, Pin.Configs.Digital);
                    // J6
                    D8 = new(8, Pin.Configs.Digital);
                    D9 = new(9, Pin.Configs.Digital);
                    D10 = new(10, Pin.Configs.Digital);
                    D11 = new(11, Pin.Configs.Digital);
                    D12 = new(12, Pin.Configs.Digital);
                    D13 = new(13, Pin.Configs.Digital);
                    D14 = new(14, Pin.Configs.Digital);
                    D15 = new(15, Pin.Configs.Digital);
                    // J8
                    D16 = new(16, Pin.Configs.Digital);
                    D17 = new(17, Pin.Configs.Digital);
                    D18 = new(18, Pin.Configs.Digital);
                    D19 = new(19, Pin.Configs.Digital);
                    D20 = new(20, Pin.Configs.Digital);
                    D21 = new(21, Pin.Configs.Digital);
                    D22 = new(22, Pin.Configs.Digital);
                    D23 = new(23, Pin.Configs.Digital);
                    D24 = new(24, Pin.Configs.Digital);
                    D25 = new(25, Pin.Configs.Digital);
                    D26 = new(26, Pin.Configs.Digital);
                    D27 = new(27, Pin.Configs.Digital);
                    // J11
                    D28 = new(28, Pin.Configs.PWM);
                    D29 = new(29, Pin.Configs.PWM);
                    D30 = new(30, Pin.Configs.PWM);
                    D31 = new(31, Pin.Configs.PWM);
                    //J9
                    // A0 = AI0
                    // A1 = AI1
                    // A2 = AI2
                    break;
            }

            Pins = new Pin[]
            {
                D0,  D1,  D2,  D3,  D4,  D5,  D6,  D7,  D8,  D9,
                D10, D11, D12, D13, D14, D15, D16, D17, D18, D19,
                D20, D21, D22, D23, D24, D25, D26, D27, D28, D29,
                D30, D31, D32, D33, D34, D35, D36, D37, D38, D39,
                D40, D41, D42, D43, D44, D45, D46, D47, D48, D49,
                D50, D51, D52, D53, D54, D55, D56, D57, D58, D59,
                A0,  A1,  A2,  A3,  A4,  A5,  A6,  A7,  A8,  A9,
                A10, A11, A12, A13, A14, A15
            };

            foreach (Pin pin in Pins)
                pin.Initialize(protocol);

            UpdateAll();

            Log.Verbose($"{boardType} created");
        }

        private void Publisher_PacketReceived(object sender, PacketReceivedArgs packet)
        {
            Pin pin = Pins[packet.Pin];
            switch (pin.Mode)
            {
                case Pin.PinModes.AnalogInput:
                    pin.Value = packet.Value;
                    break;
                case Pin.PinModes.DigitalInput:
                    pin.Value = packet.Value;
                    break;
                default:
                    break;
            }
        }

        internal void UpdateAll()
        {
            UpdateOutputs();
            UpdateInputs();
        }

        internal void UpdateOutputs()
        {
            foreach (Pin pin in Pins)
            {
                if (pin.Mode == Pin.PinModes.AnalogOutput) pin.Update();
                else if (pin.Mode == Pin.PinModes.DigitalOutput) pin.Update();
            }
        }

        internal void UpdateInputs()
        {
            byte[] data = new byte[NUM_PIN_BYTES]; // Whenever we update the inputs, we check which pins are set to be inputs
            Array.Fill(data, NULL_PLACEHOLDER);
            for (int i = 0; i < Pins.Length; i++)
            {
                Pin pin = Pins[i];
                if (pin.IsValid)
                {
                    if (pin.Mode == Pin.PinModes.DigitalInput)
                        data[pin.Number] = 1;  // Tell the Arduino to read this Digital pin
                    else if (pin.Mode == Pin.PinModes.AnalogInput)
                    {
                        int idx = (int)PinMap.A0 + 4 * (pin.Number - (int)PinMap.A0);
                        data[idx] = (byte)pin.Number;  // Tell the Arduino to read this Analog pin
                    }
                }
            }
            Protocol?.Get(data); // Send the command
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Protocol != null)
            {
                Protocol.Dispose();
                Log.Verbose($"Board disposed");
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
